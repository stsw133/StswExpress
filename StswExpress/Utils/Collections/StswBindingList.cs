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
    public StswBindingList() : base()
    {
    }
    public StswBindingList(IEnumerable<T> items) : base(items.ToList())
    {
    }
    public StswBindingList(IList<T> items) : base(items)
    {
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

            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                _itemStates[Items[e.NewIndex]] = Items[e.NewIndex].ItemState = StswItemState.Modified;
                OnPropertyChanged(nameof(Modified));
            }
            else if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                _itemStates[Items[e.NewIndex]] = Items[e.NewIndex].ItemState = StswItemState.Added;
                OnPropertyChanged(nameof(Added));
                OnPropertyChanged(nameof(Count));
            }
        }
    }

    /// <summary>
    /// Clears the collection and marks all items as "Deleted" if it was not previously "Added".
    /// </summary>
    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            if (item.ItemState != StswItemState.Added)
                _itemStates[item] = item.ItemState = StswItemState.Deleted;
            else
                _itemStates.Remove(item);
        }

        base.ClearItems();
        NotifyStateChanges();
    }
    
    /// <summary>
    /// Removes the item at the specified index from the collection and marks it as "Deleted" if it was not previously "Added".
    /// </summary>
    protected override void RemoveItem(int index)
    {
        var item = this[index];

        if (item.ItemState != StswItemState.Added)
            _itemStates[item] = item.ItemState = StswItemState.Deleted;
        else
            _itemStates.Remove(item);

        base.RemoveItem(index);
        NotifyStateChanges();
    }

    #region ItemState
    /// <summary>
    /// Gets the state of a specific collection item.
    /// </summary>
    public StswItemState GetStateOfItem(T item)
    {
        if (_itemStates.TryGetValue(item, out var state))
            return state;

        return StswItemState.Unchanged;
    }
    private readonly Dictionary<T, StswItemState> _itemStates = new();

    /// <summary>
    /// Gets a list of collection items that match the specified DataRowState.
    /// </summary>
    public IEnumerable<T> GetItemsByState(StswItemState state) => _itemStates.Where(x => x.Value == state).Select(x => x.Key);
    public int Added => GetItemsByState(StswItemState.Added).Count();
    public int Modified => GetItemsByState(StswItemState.Modified).Count();
    public int Deleted => GetItemsByState(StswItemState.Deleted).Count();

    /// <summary>
    /// Gets or sets a list of property names to be ignored during state tracking.
    /// </summary>
    public List<string> IgnoredProperties = new List<string>()
    {
        nameof(IStswCollectionItem.ItemMessage),
        nameof(IStswCollectionItem.ItemState),
        nameof(IStswCollectionItem.ShowDetails),
        nameof(IStswSelectionItem.IsSelected)
    };

    /// Notify the view that the ItemStates property has changed
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// 
    /// </summary>
    private void NotifyStateChanges()
    {
        OnPropertyChanged(nameof(Added));
        OnPropertyChanged(nameof(Modified));
        OnPropertyChanged(nameof(Deleted));
        OnPropertyChanged(nameof(Count));
    }
    #endregion

    #region Sorting
    private bool _isSorted;
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;
    private PropertyDescriptor? _sortProperty;

    protected override bool SupportsSortingCore => true;
    protected override bool IsSortedCore => _isSorted;
    protected override ListSortDirection SortDirectionCore => _sortDirection;
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
    #endregion
}

/// <summary>
/// Provides a custom comparer for sorting elements based on a specified property.
/// </summary>
/// <typeparam name="T">The type of elements to compare.</typeparam>
internal class PropertyComparer<T> : IComparer<T>
{
    private readonly PropertyDescriptor _property;
    private readonly ListSortDirection _direction;

    public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
    {
        _property = property;
        _direction = direction;
    }

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
    public int Compare(T? x, T? y)
    {
        if (_property.GetValue(x) is not IComparable xValue || _property.GetValue(y) is not IComparable yValue)
            return 0;

        if (_direction == ListSortDirection.Ascending)
            return xValue.CompareTo(yValue);
        else
            return yValue.CompareTo(xValue);
    }
}
