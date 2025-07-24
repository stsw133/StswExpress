using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StswExpress.Commons;

/// <summary>
/// A custom <see cref="ObservableCollection{T}"/> that tracks changes on items and
/// maintains separate lists for added, removed, and modified items.
/// Also allows ignoring certain property names to prevent marking
/// items as modified when those properties change.
/// </summary>
/// <typeparam name="T">Item type implementing <see cref="IStswCollectionItem"/></typeparam>
[StswInfo("0.15.0")] //TODO - handle changes in nested classes
public class StswObservableCollection<T> : ObservableCollection<T> where T : IStswCollectionItem
{
    private bool _isBulkLoading;

    /// <summary>
    /// Initializes a new instance of <see cref="StswObservableCollection{T}"/>.
    /// </summary>
    public StswObservableCollection() : base() { }

    /// <summary>
    /// Initializes a new instance of <see cref="StswObservableCollection{T}"/> with an initial collection of items.
    /// </summary>
    /// <param name="collection">Initial collection of items.</param>
    public StswObservableCollection(IEnumerable<T> collection) : base()
    {
        if (collection != null)
        {
            _isBulkLoading = true;

            foreach (var item in collection)
            {
                Items.Add(item);
                item.PropertyChanged += OnItemPropertyChanged;
            }

            _isBulkLoading = false;
        }

        RecountStates();

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    #region Events & methods
    /// <summary>
    /// Accepts all changes made to the items in the collection.
    /// </summary>
    public void AcceptChanges()
    {
        foreach (var item in _addedItems)
            item.ItemState = StswItemState.Unchanged;

        foreach (var item in _deletedItems)
            item.ItemState = StswItemState.Unchanged;

        foreach (var item in _modifiedItems)
            item.ItemState = StswItemState.Unchanged;

        _addedItems.Clear();
        _deletedItems.Clear();
        _modifiedItems.Clear();

        RecountStates();
    }

    /// <summary>
    /// Adds a range of items to the collection. Each item is marked with the specified state.
    /// </summary>
    /// <param name="items">The items to add to the collection.</param>
    /// <param name="itemsState">The state to assign to each item. Default is Added.</param>
    public void AddRange(IEnumerable<T> items, StswItemState itemsState = StswItemState.Added)
    {
        if (items.IsNullOrEmpty())
            return;

        CheckReentrancy();

        foreach (var item in items)
        {
            if (Items.Contains(item))
                continue;

            item.ItemState = itemsState;
            Add(item);
            item.PropertyChanged += OnItemPropertyChanged;
        }

        RecountStates();
    }

    /// <summary>
    /// Adds a range of items to the collection without triggering change notifications.
    /// </summary>
    /// <param name="items">The items to add to the collection.</param>
    /// <param name="itemsState">The state to assign to each item. Default is Added.</param>
    public void AddRangeFast(IEnumerable<T> items, StswItemState itemsState = StswItemState.Added)
    {
        if (items.IsNullOrEmpty())
            return;

        CheckReentrancy();
        _isBulkLoading = true;

        foreach (var item in items)
        {
            if (Items.Contains(item))
                continue;

            item.ItemState = itemsState;
            Items.Add(item);
            item.PropertyChanged += OnItemPropertyChanged;
        }

        _isBulkLoading = false;

        RecountStates();
        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <inheritdoc/>
    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            item.PropertyChanged -= OnItemPropertyChanged;

            if (item.ItemState == StswItemState.Added && _addedItems.Contains(item))
            {
                _addedItems.Remove(item);
            }
            else
            {
                if (item.ItemState != StswItemState.Deleted)
                {
                    item.ItemState = StswItemState.Deleted;
                    _deletedItems.AddIfNotContains(item);
                }

                _modifiedItems.Remove(item);
            }
        }

        base.ClearItems();

        RecountStates();
    }

    /// <inheritdoc/>
    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);
        item.PropertyChanged += OnItemPropertyChanged;

        if (!_isBulkLoading)
            item.ItemState = StswItemState.Added;

        RecountStates();
    }

    /// <inheritdoc/>
    protected override void RemoveItem(int index)
    {
        var item = this[index];

        if (ShowRemovedItems)
        {
            var oldState = item.ItemState;
            if (oldState != StswItemState.Added)
            {
                item.ItemState = StswItemState.Deleted;
                _deletedItems.AddIfNotContains(item);
            }
            else
            {
                _addedItems.Remove(item);
            }
        }
        else
        {
            base.RemoveItem(index);

            var oldState = item.ItemState;

            item.PropertyChanged -= OnItemPropertyChanged;

            if (item.ItemState == StswItemState.Added && _addedItems.Contains(item))
            {
                _addedItems.Remove(item);
            }
            else
            {
                if (item.ItemState != StswItemState.Deleted)
                {
                    item.ItemState = StswItemState.Deleted;
                    _deletedItems.AddIfNotContains(item);
                }

                _modifiedItems.Remove(item);
            }
        }

        RecountStates();
    }

    /// <inheritdoc/>
    protected override void SetItem(int index, T item)
    {
        var oldItem = this[index];
        RemoveItem(index);

        base.SetItem(index, item);
        item.PropertyChanged += OnItemPropertyChanged;

        if (!_isBulkLoading)
            item.ItemState = StswItemState.Added;

        RecountStates();
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Handles the state change of an item in the collection.
    /// </summary>
    /// <param name="item">The item whose state has changed.</param>
    [StswInfo("0.19.0")]
    private void HandleItemStateChange(T item)
    {
        _addedItems.Remove(item);
        _modifiedItems.Remove(item);
        _deletedItems.Remove(item);

        switch (item.ItemState)
        {
            case StswItemState.Added:
                _addedItems.AddIfNotContains(item);
                break;
            case StswItemState.Modified:
                _modifiedItems.AddIfNotContains(item);
                break;
            case StswItemState.Deleted:
                _deletedItems.AddIfNotContains(item);
                break;
        }

        RecountStates();
    }

    /// <summary>
    /// Event handler for property changes of items in the collection.
    /// </summary>
    /// <param name="sender">The item that raised the event.</param>
    /// <param name="e">PropertyChangedEventArgs details.</param>
    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not T trackableItem)
            return;

        if (e.PropertyName == nameof(trackableItem.ItemState))
        {
            HandleItemStateChange(trackableItem);
            return;
        }

        if (IgnoredPropertyNames.Contains(e.PropertyName!))
            return;

        if (trackableItem.ItemState == StswItemState.Unchanged)
            trackableItem.ItemState = StswItemState.Modified;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets a read-only collection of items in the <see cref="StswItemState.Unchanged"/> state."/>
    /// </summary>
    public IEnumerable<T> UnchangedItems => this.Where(x => x.ItemState == StswItemState.Unchanged);

    /// <summary>
    /// Gets a read-only collection of items in the <see cref="StswItemState.Added"/> state."/>
    /// </summary>
    public IReadOnlyList<T> AddedItems => (IReadOnlyList<T>)_addedItems;
    private readonly IList<T> _addedItems = [];

    /// <summary>
    /// Gets a read-only collection of items in the <see cref="StswItemState.Deleted"/> state.
    /// </summary>
    public IReadOnlyList<T> DeletedItems => (IReadOnlyList<T>)_deletedItems;
    private readonly IList<T> _deletedItems = [];

    /// <summary>
    /// Gets a read-only collection of items in the <see cref="StswItemState.Modified"/> state.
    /// </summary>
    public IReadOnlyList<T> ModifiedItems => (IReadOnlyList<T>)_modifiedItems;
    private readonly IList<T> _modifiedItems = [];

    /// <summary>
    /// Gets the collection of ignored property names when tracking modifications.
    /// </summary>
    public HashSet<string> IgnoredPropertyNames { get; set; } =
    [
        nameof(IStswCollectionItem.ShowDetails),
        nameof(IStswSelectionItem.IsSelected),
    ];

    /// <summary>
    /// Indicates whether removed items should be tracked in the collection.
    /// </summary>
    public bool ShowRemovedItems { get; set; } = false;
    #endregion

    #region Counters
    private bool _countersAreDirty = true;

    private int _countUnchanged;
    private int _countAdded;
    private int _countDeleted;
    private int _countModified;

    /// <summary>
    /// Gets the number of items in the Unchanged state.
    /// </summary>
    public int CountUnchanged
    {
        get
        {
            EnsureCountersAreUpToDate();
            return _countUnchanged;
        }
        private set
        {
            if (_countUnchanged != value)
            {
                _countUnchanged = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the number of items in the Added state.
    /// </summary>
    public int CountAdded
    {
        get
        {
            EnsureCountersAreUpToDate();
            return _countAdded;
        }
        private set
        {
            if (_countAdded != value)
            {
                _countAdded = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the number of items in the Deleted state.
    /// </summary>
    public int CountDeleted
    {
        get
        {
            EnsureCountersAreUpToDate();
            return _countDeleted;
        }
        private set
        {
            if (_countDeleted != value)
            {
                _countDeleted = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the number of items in the Modified state.
    /// </summary>
    public int CountModified
    {
        get
        {
            EnsureCountersAreUpToDate();
            return _countModified;
        }
        private set
        {
            if (_countModified != value)
            {
                _countModified = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Ensures the counters are up-to-date. If they are dirty, it recalculates them.
    /// </summary>
    private void EnsureCountersAreUpToDate()
    {
        if (_countersAreDirty)
        {
            _countersAreDirty = false;
            RecountStates();
        }
    }

    /// <summary>
    /// Triggers change notifications for property changes.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Recounts the states of the items in the collection and updates the counters accordingly.
    /// </summary>
    private void RecountStates()
    {
        if (_countersAreDirty)
            return;

        if (ShowRemovedItems)
            CountUnchanged = Items.Count - _addedItems.Count - _modifiedItems.Count - _deletedItems.Count;
        else
            CountUnchanged = Items.Count - _addedItems.Count - _modifiedItems.Count;

        CountAdded = _addedItems.Count;
        CountModified = _modifiedItems.Count;
        CountDeleted = _deletedItems.Count;
    }
    #endregion
}

