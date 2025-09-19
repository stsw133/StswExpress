using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StswExpress.Commons;

/// <summary>
/// A custom <see cref="ObservableCollection{T}"/> that tracks changes on items and maintains separate lists for added, removed, and modified items.
/// Also allows ignoring certain property names to prevent marking items as modified when those properties change.
/// </summary>
/// <typeparam name="T">Item type implementing <see cref="IStswCollectionItem"/></typeparam>
[StswInfo("0.15.0", "0.20.1")] //TODO - handle changes in nested classes
public class StswObservableCollection<T> : ObservableCollection<T> where T : IStswCollectionItem
{
    private bool _isBulkLoading;

    /// <summary>
    /// Initializes a new instance of <see cref="StswObservableCollection{T}"/>.
    /// </summary>
    /// <param name="trackItems">If set to <see langword="true"/>, the collection will track changes to the items.</param>
    [StswInfo("0.15.0", "0.20.1")]
    public StswObservableCollection(bool trackItems = true) : this([])
    {
        TrackItems = trackItems;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StswObservableCollection{T}"/> with an initial collection of items.
    /// </summary>
    /// <param name="collection">The initial items to populate the collection.</param>
    /// <param name="trackItems">If set to <see langword="true"/>, the collection will track changes to the items.</param>
    [StswInfo("0.15.0", "0.20.1")]
    public StswObservableCollection(IEnumerable<T> collection, bool trackItems = true) : base()
    {
        TrackItems = trackItems;

        if (collection != null)
        {
            _isBulkLoading = true;

            foreach (var item in collection)
            {
                Items.Add(item);
                if (TrackItems)
                    item.PropertyChanged += OnItemPropertyChanged;
            }

            _isBulkLoading = false;
        }

        if (TrackItems)
            RebuildTrackingListsFromStates();

        UpdateCountersIfEnabled();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    #region Events & methods
    /// <summary>
    /// Accepts all changes made to the items in the collection.
    /// </summary>
    [StswInfo("0.15.0", "0.20.1")]
    public void AcceptChanges()
    {
        if (TrackItems)
        {
            foreach (var item in _addedItems.ToList())
                item.ItemState = StswItemState.Unchanged;

            foreach (var item in _deletedItems.ToList())
                item.ItemState = StswItemState.Unchanged;

            foreach (var item in _modifiedItems.ToList())
                item.ItemState = StswItemState.Unchanged;
        }
        Items.ForEach(item => item.ItemState = StswItemState.Unchanged);

        _addedItems.Clear();
        _deletedItems.Clear();
        _modifiedItems.Clear();

        UpdateCountersIfEnabled();
    }

    /// <summary>
    /// Adds a range of items to the collection. Each item is marked with the specified state.
    /// </summary>
    /// <param name="items">The items to add to the collection.</param>
    [StswInfo("0.15.0", "0.20.1")]
    public void AddRange(IEnumerable<T> items)
    {
        if (items.IsNullOrEmpty())
            return;

        CheckReentrancy();

        foreach (var item in items)
        {
            if (Items.Contains(item))
                continue;

            Add(item);
            if (TrackItems)
                HandleItemStateChange(item);
        }

        UpdateCountersIfEnabled();
    }

    /// <summary>
    /// Adds a range of items to the collection without triggering change notifications.
    /// </summary>
    /// <param name="items">The items to add to the collection.</param>
    /// <param name="itemState">The state to assign to each item. Default is Added.</param>
    [StswInfo("0.19.1", "0.20.1")]
    public void AddRangeFast(IEnumerable<T> items, StswItemState? itemState = null)
    {
        if (items.IsNullOrEmpty())
            return;

        CheckReentrancy();
        _isBulkLoading = true;
        var addedAny = false;

        foreach (var item in items)
        {
            if (Items.Contains(item))
                continue;

            Items.Add(item);

            if (TrackItems)
            {
                item.PropertyChanged += OnItemPropertyChanged;

                var targetState = itemState ?? item.ItemState;
                if (!EqualityComparer<StswItemState>.Default.Equals(item.ItemState, targetState))
                    item.ItemState = targetState;
                else
                    HandleItemStateChange(item);
            }

            addedAny = true;
        }

        _isBulkLoading = false;

        if (addedAny)
        {
            UpdateCountersIfEnabled();
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    /// <inheritdoc/>
    [StswInfo("0.15.0", "0.20.0")]
    protected override void ClearItems()
    {
        foreach (var item in this)
            item.PropertyChanged -= OnItemPropertyChanged;

        base.ClearItems();

        _addedItems.Clear();
        _modifiedItems.Clear();
        _deletedItems.Clear();

        UpdateCountersIfEnabled();
    }

    /// <summary>
    /// Deletes all items in the collection by marking them as deleted.
    /// </summary>
    [StswInfo("0.20.0", "0.20.1")]
    public void DeleteItems()
    {
        if (Count == 0)
            return;

        if (!TrackItems)
        {
            base.ClearItems();
            _addedItems.Clear();
            _modifiedItems.Clear();
            _deletedItems.Clear();
            UpdateCountersIfEnabled();
            return;
        }

        foreach (var item in this)
        {
            item.PropertyChanged -= OnItemPropertyChanged;

            if (item.ItemState == StswItemState.Added)
            {
                _addedItems.Remove(item);
            }
            else
            {
                item.ItemState = StswItemState.Deleted;
                _deletedItems.AddIfNotContains(item);
                _modifiedItems.Remove(item);
            }
        }

        if (!ShowRemovedItems)
            base.ClearItems();

        UpdateCountersIfEnabled();
    }

    /// <inheritdoc/>
    [StswInfo("0.15.0", "0.20.1")]
    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);

        if (TrackItems)
        {
            item.PropertyChanged += OnItemPropertyChanged;

            if (!_isBulkLoading)
                item.ItemState = StswItemState.Added;

            UpdateCountersIfEnabled();
        }
    }

    /// <inheritdoc/>
    [StswInfo("0.15.0", "0.20.1")]
    protected override void RemoveItem(int index)
    {
        var item = this[index];

        if (!TrackItems)
        {
            base.RemoveItem(index);
            item.PropertyChanged -= OnItemPropertyChanged;
            UpdateCountersIfEnabled();
            return;
        }

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

        UpdateCountersIfEnabled();
    }

    /// <inheritdoc/>
    [StswInfo("0.15.0", "0.20.1")]
    protected override void SetItem(int index, T item)
    {
        var oldItem = this[index];

        if (!TrackItems)
        {
            base.SetItem(index, item);
            UpdateCountersIfEnabled();
            return;
        }

        RemoveItem(index);

        base.SetItem(index, item);
        item.PropertyChanged += OnItemPropertyChanged;

        if (!_isBulkLoading)
            item.ItemState = StswItemState.Added;

        UpdateCountersIfEnabled();
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Handles the state change of an item in the collection.
    /// </summary>
    /// <param name="item">The item whose state has changed.</param>
    [StswInfo("0.19.0", "0.20.1")]
    private void HandleItemStateChange(T item)
    {
        if (!TrackItems)
            return;

        _addedItems.Remove(item);
        _modifiedItems.Remove(item);
        _deletedItems.Remove(item);

        switch (item.ItemState)
        {
            case StswItemState.Added:
                _addedItems.Add(item);
                break;
            case StswItemState.Modified:
                _modifiedItems.Add(item);
                break;
            case StswItemState.Deleted:
                _deletedItems.Add(item);
                break;
        }

        UpdateCountersIfEnabled();
    }

    /// <summary>
    /// Event handler for property changes of items in the collection.
    /// </summary>
    /// <param name="sender">The item that raised the event.</param>
    /// <param name="e">PropertyChangedEventArgs details.</param>
    [StswInfo("0.15.0", "0.20.1")]
    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!TrackItems)
            return;

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

    /// <summary>
    /// Rebuilds the tracking lists (_addedItems, _modifiedItems, _deletedItems) based on the current states of the items in the collection.
    /// </summary>
    [StswInfo("0.20.1")]
    private void RebuildTrackingListsFromStates()
    {
        _addedItems.Clear();
        _modifiedItems.Clear();
        _deletedItems.Clear();

        if (!TrackItems)
            return;

        foreach (var item in this)
        {
            switch (item.ItemState)
            {
                case StswItemState.Added:
                    _addedItems.Add(item);
                    break;
                case StswItemState.Modified:
                    _modifiedItems.Add(item);
                    break;
                case StswItemState.Deleted:
                    _deletedItems.Add(item);
                    break;
            }
        }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Controls whether the collection tracks item state changes and mutates <see cref="IStswCollectionItem.ItemState"/>.
    /// When set to false, the collection behaves like a plain <see cref="ObservableCollection{T}"/> with no item-state updates and no tracking lists.
    /// Toggling this property re-wires subscriptions and rebuilds tracking lists/counters accordingly.
    /// </summary>
    [StswInfo("0.20.1")]
    public bool TrackItems
    {
        get => _trackItems;
        set
        {
            if (_trackItems == value)
                return;

            _trackItems = value;

            if (_trackItems)
            {
                foreach (var item in this)
                    item.PropertyChanged += OnItemPropertyChanged;

                RebuildTrackingListsFromStates();
            }
            else
            {
                foreach (var item in this)
                    item.PropertyChanged -= OnItemPropertyChanged;

                _addedItems.Clear();
                _modifiedItems.Clear();
                _deletedItems.Clear();
            }

            UpdateCountersIfEnabled();
            OnPropertyChanged();
        }
    }
    private bool _trackItems = true;

    /// <summary>
    /// Gets a read-only collection of items in the <see cref="StswItemState.Unchanged"/> state."/>
    /// </summary>
    [StswInfo("0.15.0", "0.19.0")]
    public IEnumerable<T> UnchangedItems => this.Where(x => x.ItemState == StswItemState.Unchanged);

    /// <summary>
    /// Gets a read-only collection of items in the <see cref="StswItemState.Added"/> state."/>
    /// </summary>
    [StswInfo("0.15.0", "0.19.0")]
    public IReadOnlyList<T> AddedItems => (IReadOnlyList<T>)_addedItems;
    private readonly IList<T> _addedItems = [];

    /// <summary>
    /// Gets a read-only collection of items in the <see cref="StswItemState.Deleted"/> state.
    /// </summary>
    [StswInfo("0.15.0", "0.19.0")]
    public IReadOnlyList<T> DeletedItems => (IReadOnlyList<T>)_deletedItems;
    private readonly IList<T> _deletedItems = [];

    /// <summary>
    /// Gets a read-only collection of items in the <see cref="StswItemState.Modified"/> state.
    /// </summary>
    [StswInfo("0.15.0", "0.19.0")]
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
    /// <summary>
    /// Gets the number of items in the Unchanged state.
    /// </summary>
    public int CountUnchanged
    {
        get
        {
            EnsureCountersEnabled();
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
    private int _countUnchanged;

    /// <summary>
    /// Gets the number of items in the Added state.
    /// </summary>
    public int CountAdded
    {
        get
        {
            EnsureCountersEnabled();
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
    private int _countAdded;

    /// <summary>
    /// Gets the number of items in the Deleted state.
    /// </summary>
    public int CountDeleted
    {
        get
        {
            EnsureCountersEnabled();
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
    private int _countDeleted;

    /// <summary>
    /// Gets the number of items in the Modified state.
    /// </summary>
    public int CountModified
    {
        get
        {
            EnsureCountersEnabled();
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
    private int _countModified;

    /// <summary>
    /// Ensures the counters are up-to-date. If they are dirty, it recalculates them.
    /// </summary>
    [StswInfo("0.15.0", "0.20.0")]
    private void EnsureCountersEnabled()
    {
        if (_countersEnabled) return;
        _countersEnabled = true;
        UpdateCountersIfEnabled();
    }
    private bool _countersEnabled;

    /// <summary>
    /// Triggers change notifications for property changes.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Recounts the states of the items in the collection and updates the counters accordingly.
    /// </summary>
    [StswInfo("0.15.0", "0.20.1")]
    private void UpdateCountersIfEnabled()
    {
        if (!_countersEnabled)
            return;

        if (!TrackItems)
        {
            CountUnchanged = Items.Count;
            CountAdded = 0;
            CountModified = 0;
            CountDeleted = 0;
            return;
        }

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

