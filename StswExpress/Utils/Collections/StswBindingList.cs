using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace StswExpress;
/// <summary>
/// An extension of <see cref="BindingList{T}"/> that adds tracking of changes to the collection items' states.
/// </summary>
public class StswBindingList<T> : BindingList<T>, INotifyPropertyChanged where T : IStswCollectionItem
{
    public StswBindingList() : base() => Initialize();
    public StswBindingList(IEnumerable<T> items) : base(items.ToList()) => Initialize();
    public StswBindingList(IList<T> items) : base(items) => Initialize();

    /// <summary>
    /// Initializes the list by setting up item states.
    /// </summary>
    private void Initialize()
    {
        foreach (var item in this)
            if (!ItemStates.ContainsKey(item))
                ItemStates[item] = item.ItemState;
        NotifyStateChanges();
    }

    /// <summary>
    /// Applies changes to the state of collection items and notifies listeners after a change has occurred.
    /// </summary>
    /// <param name="e">The event arguments that describe the change.</param>
    protected override void OnListChanged(ListChangedEventArgs e)
    {
        base.OnListChanged(e);

        if (e == null || e.ListChangedType.In(ListChangedType.ItemDeleted, ListChangedType.Reset))
            return;

        if (Items[e.NewIndex] is T item && item.ItemState == StswItemState.Unchanged)
        {
            if (e.PropertyDescriptor?.Name?.In(IgnoredProperties) == true)
                return;

            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    ItemStates[Items[e.NewIndex]] = Items[e.NewIndex].ItemState = StswItemState.Added;
                    break;
                case ListChangedType.ItemDeleted:
                    // Item is already removed from the list, so we can't access it via Items[e.NewIndex]
                    // Handle this case in RemoveItem
                    break;
                case ListChangedType.ItemChanged:
                    ItemStates[Items[e.NewIndex]] = Items[e.NewIndex].ItemState = StswItemState.Modified;
                    break;
            }
        }
        NotifyStateChanges();
    }

    #region Base methods
    /// <summary>
    /// Clears the collection and marks all items as "Deleted" if it was not previously "Added".
    /// </summary>
    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            if (item.ItemState != StswItemState.Added)
                ItemStates[item] = item.ItemState = StswItemState.Deleted;
            else
                ItemStates.Remove(item);
        }

        base.ClearItems();
        NotifyStateChanges();
    }

    /// <summary>
    /// Removes the item at the specified index from the collection and marks it as "Deleted" if it was not previously "Added".
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    protected override void RemoveItem(int index)
    {
        var item = this[index];

        if (item.ItemState != StswItemState.Added)
            ItemStates[item] = item.ItemState = StswItemState.Deleted;
        else
            ItemStates.Remove(item);

        base.RemoveItem(index);
        NotifyStateChanges();
    }

    /// <summary>
    /// Adds the item to the collection and sets its state to the specific one.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="itemState">The state to set for the item.</param>
    public void Add(T item, StswItemState itemState)
    {
        Add(item);
        ItemStates[item] = item.ItemState = itemState;
        NotifyStateChanges();
    }

    /// <summary>
    /// Adds a range of items to the collection and sets their states to the specific one.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <param name="itemState">The state to set for the items.</param>
    public void AddRange(IEnumerable<T> items, StswItemState? itemState = null)
    {
        foreach (var item in items)
        {
            Add(item);
            if (itemState != null)
                ItemStates[item] = item.ItemState = itemState.Value;
        }
        NotifyStateChanges();
    }

    /// <summary>
    /// Adds a range of items to the collection and sets their states to the specific one.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <param name="itemState">The state to set for the items.</param>
    public void AddRange(IList<T> items, StswItemState? itemState = null) => AddRange(items.AsEnumerable(), itemState);
    #endregion

    #region Item state management
    /// <summary>
    /// Gets or sets the dictionary that maps items to their states.
    /// </summary>
    public Dictionary<T, StswItemState> ItemStates { get; private set; } = [];

    /// <summary>
    /// Gets the state of a specific collection item.
    /// </summary>
    /// <param name="item">The item to get the state of.</param>
    /// <returns>The state of the item.</returns>
    public StswItemState GetStateOfItem(T item) => ItemStates.TryGetValue(item, out var state) ? state : StswItemState.Unchanged;

    /// <summary>
    /// Gets a list of collection items that match the specified item state.
    /// </summary>
    /// <param name="state">The state to filter items by.</param>
    /// <returns>A list of items that match the specified state.</returns>
    public IEnumerable<T> GetItemsByState(StswItemState state) => ItemStates.Where(x => x.Value == state).Select(x => x.Key);

    /// <summary>
    /// Gets the count of items in an unchanged state.
    /// </summary>
    public int Unchanged => GetItemsByState(StswItemState.Unchanged).Count();

    /// <summary>
    /// Gets the count of items in a modified state.
    /// </summary>
    public int Modified => GetItemsByState(StswItemState.Modified).Count();
    
    /// <summary>
    /// Gets the count of items in an added state.
    /// </summary>
    public int Added => GetItemsByState(StswItemState.Added).Count();
    
    /// <summary>
    /// Gets the count of items in a deleted state.
    /// </summary>
    public int Deleted => GetItemsByState(StswItemState.Deleted).Count();

    /// <summary>
    /// Gets or sets a list of property names to be ignored during state tracking.
    /// </summary>
    public List<string> IgnoredProperties =
    [
        nameof(IStswCollectionItem.ItemState),
        nameof(IStswCollectionItem.ShowDetails),
        nameof(IStswSelectionItem.IsSelected)
    ];

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    
    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Notifies that the state of the items has changed.
    /// </summary>
    private void NotifyStateChanges()
    {
        OnPropertyChanged(nameof(Unchanged));
        OnPropertyChanged(nameof(Added));
        OnPropertyChanged(nameof(Deleted));
        OnPropertyChanged(nameof(Modified));
        OnPropertyChanged(nameof(Count));
    }
    #endregion

    #region Sorting
    private bool _isSorted;
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;
    private PropertyDescriptor? _sortProperty;

    /// <summary>
    /// Gets a value indicating whether the list supports sorting.
    /// </summary>
    protected override bool SupportsSortingCore => true;
    
    /// <summary>
    /// Gets a value indicating whether the list is currently sorted.
    /// </summary>
    protected override bool IsSortedCore => _isSorted;
    
    /// <summary>
    /// Gets the direction of the sort.
    /// </summary>
    protected override ListSortDirection SortDirectionCore => _sortDirection;
    
    /// <summary>
    /// Gets the property descriptor used for sorting.
    /// </summary>
    protected override PropertyDescriptor? SortPropertyCore => _sortProperty;

    /// <summary>
    /// Sorts the elements in the list based on the specified property descriptor and direction.
    /// </summary>
    /// <param name="prop">The property descriptor to sort by.</param>
    /// <param name="direction">The direction of the sort.</param>
    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
    {
        if (Items is not List<T> itemsList)
            return;

        var comparer = new PropertyComparer<T>(prop, direction);
        itemsList.Sort(comparer);

        _sortProperty = prop;
        _sortDirection = direction;
        _isSorted = true;

        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    /// <summary>
    /// Removes any sort applied to the list.
    /// </summary>
    protected override void RemoveSortCore()
    {
        _isSorted = false;
        _sortProperty = null;
    }

    /// <summary>
    /// Provides a custom comparer for sorting elements based on a specified property.
    /// </summary>
    /// <typeparam name="T2">The type of elements to compare.</typeparam>
    /// <param name="property">The property descriptor to sort by.</param>
    /// <param name="direction">The direction of the sort.</param>
    private class PropertyComparer<T2>(PropertyDescriptor property, ListSortDirection direction) : IComparer<T2>
    {
        private readonly PropertyDescriptor _property = property;
        private readonly ListSortDirection _direction = direction;

        /// <summary>
        /// Compares two elements based on the specified property and sort direction.
        /// </summary>
        /// <param name="x">The first element to compare.</param>
        /// <param name="y">The second element to compare.</param>
        /// <returns>
        /// A negative value if <paramref name="x"/> is less than <paramref name="y"/>;
        /// zero if <paramref name="x"/> equals <paramref name="y"/>;
        /// a positive value if <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        public int Compare(T2? x, T2? y)
        {
            if (_property.GetValue(x) is not IComparable xValue || _property.GetValue(y) is not IComparable yValue)
                return 0;

            if (_direction == ListSortDirection.Ascending)
                return xValue.CompareTo(yValue);
            else
                return yValue.CompareTo(xValue);
        }
    }
    #endregion
}
