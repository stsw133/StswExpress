using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StswExpress.Commons;

/// <summary>
/// A custom <see cref="ObservableCollection{T}"/> that tracks changes on items and maintains separate lists for added, removed, and modified items.
/// Also allows ignoring certain property names to prevent marking items as modified when those properties change.
/// </summary>
/// <typeparam name="T">Item type stored in the collection.</typeparam>
[StswPlannedChanges(StswPlannedChanges.NewFeatures, "Handling of nested class changes is not yet implemented.")]
public class StswObservableCollection<T> : ObservableCollection<T>
{
    private bool _isBulkLoading;

    /// <summary>
    /// Initializes a new instance of <see cref="StswObservableCollection{T}"/>.
    /// </summary>
    public StswObservableCollection() : this([])
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StswObservableCollection{T}"/>.
    /// </summary>
    /// <param name="trackItems">If set to <see langword="true"/>, the collection will track changes to the items.</param>
    public StswObservableCollection(bool trackItems = true) : this([])
    {
        TrackItems = trackItems;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StswObservableCollection{T}"/> with an initial collection of items.
    /// </summary>
    /// <param name="collection">The initial items to populate the collection.</param>
    /// <param name="trackItems">If set to <see langword="true"/>, the collection will track changes to the items.</param>
    public StswObservableCollection(IEnumerable<T> collection, bool trackItems = true) : base()
    {
        TrackItems = trackItems;

        if (collection != null)
        {
            _isBulkLoading = true;

            foreach (var item in collection)
            {
                Items.Add(item);
                if (TrackItems && item is IStswTrackableItem trackableItem)
                    trackableItem.PropertyChanged += OnItemPropertyChanged;
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
    public void AcceptChanges()
    {
        if (TrackItems)
        {
            foreach (var item in _addedItems.OfType<IStswTrackableItem>().ToList())
                item.ItemState = StswItemState.Unchanged;

            foreach (var item in _deletedItems.OfType<IStswTrackableItem>().ToList())
                item.ItemState = StswItemState.Unchanged;

            foreach (var item in _modifiedItems.OfType<IStswTrackableItem>().ToList())
                item.ItemState = StswItemState.Unchanged;
        }

        foreach (var item in Items.OfType<IStswTrackableItem>())
            item.ItemState = StswItemState.Unchanged;

        _addedItems.Clear();
        _deletedItems.Clear();
        _modifiedItems.Clear();

        UpdateCountersIfEnabled();
    }

    /// <summary>
    /// Adds a range of items to the collection. Each item is marked with the specified state.
    /// </summary>
    /// <param name="items">The items to add to the collection.</param>
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
            if (TrackItems && item is IStswTrackableItem trackableItem)
                HandleItemStateChange(trackableItem);
        }

        UpdateCountersIfEnabled();
    }

    /// <summary>
    /// Adds a range of items to the collection without triggering change notifications.
    /// </summary>
    /// <param name="items">The items to add to the collection.</param>
    /// <param name="itemState">The state to assign to each item. Default is Added.</param>
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

            if (TrackItems && item is IStswTrackableItem trackableItem)
            {
                trackableItem.PropertyChanged += OnItemPropertyChanged;

                var targetState = itemState ?? trackableItem.ItemState;
                if (!EqualityComparer<StswItemState>.Default.Equals(trackableItem.ItemState, targetState))
                    trackableItem.ItemState = targetState;
                else
                    HandleItemStateChange(trackableItem);
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
    protected override void ClearItems()
    {
        foreach (var item in this.OfType<IStswTrackableItem>())
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
            if (item is not IStswTrackableItem trackableItem)
                continue;

            trackableItem.PropertyChanged -= OnItemPropertyChanged;

            if (trackableItem.ItemState == StswItemState.Added)
            {
                _addedItems.Remove(item);
            }
            else
            {
                trackableItem.ItemState = StswItemState.Deleted;
                _deletedItems.AddIfNotContains(item);
                _modifiedItems.Remove(item);
            }
        }

        if (!ShowRemovedItems)
            base.ClearItems();

        UpdateCountersIfEnabled();
    }

    /// <inheritdoc/>
    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);

        if (TrackItems && item is IStswTrackableItem trackableItem)
        {
            trackableItem.PropertyChanged += OnItemPropertyChanged;

            if (!_isBulkLoading)
                trackableItem.ItemState = StswItemState.Added;

            UpdateCountersIfEnabled();
        }
    }

    /// <inheritdoc/>
    protected override void RemoveItem(int index)
    {
        var item = this[index];

        if (!TrackItems || item is not IStswTrackableItem trackableItem)
        {
            base.RemoveItem(index);
            if (item is IStswTrackableItem observableItem)
                observableItem.PropertyChanged -= OnItemPropertyChanged;
            UpdateCountersIfEnabled();
            return;
        }

        if (ShowRemovedItems)
        {
            var oldState = trackableItem.ItemState;
            if (oldState != StswItemState.Added)
            {
                trackableItem.ItemState = StswItemState.Deleted;
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

            trackableItem.PropertyChanged -= OnItemPropertyChanged;

            if (trackableItem.ItemState == StswItemState.Added && _addedItems.Contains(item))
            {
                _addedItems.Remove(item);
            }
            else
            {
                if (trackableItem.ItemState != StswItemState.Deleted)
                {
                    trackableItem.ItemState = StswItemState.Deleted;
                    _deletedItems.AddIfNotContains(item);
                }

                _modifiedItems.Remove(item);
            }
        }

        UpdateCountersIfEnabled();
    }

    /// <inheritdoc/>
    protected override void SetItem(int index, T item)
    {
        var oldItem = this[index];

        if (!TrackItems)
        {
            base.SetItem(index, item);
            UpdateCountersIfEnabled();
            return;
        }

        if (oldItem is IStswTrackableItem oldTrackable)
        {
            oldTrackable.PropertyChanged -= OnItemPropertyChanged;

            if (ShowRemovedItems)
            {
                if (oldTrackable.ItemState != StswItemState.Added)
                {
                    oldTrackable.ItemState = StswItemState.Deleted;
                    _deletedItems.AddIfNotContains(oldItem);
                }
                else
                {
                    _addedItems.Remove(oldItem);
                }
            }
            else
            {
                if (oldTrackable.ItemState == StswItemState.Added && _addedItems.Contains(oldItem))
                {
                    _addedItems.Remove(oldItem);
                }
                else
                {
                    if (oldTrackable.ItemState != StswItemState.Deleted)
                    {
                        oldTrackable.ItemState = StswItemState.Deleted;
                        _deletedItems.AddIfNotContains(oldItem);
                    }

                    _modifiedItems.Remove(oldItem);
                }
            }
        }

        base.SetItem(index, item);
        if (item is IStswTrackableItem newTrackable)
        {
            newTrackable.PropertyChanged += OnItemPropertyChanged;

            if (!_isBulkLoading)
                newTrackable.ItemState = StswItemState.Added;
        }

        UpdateCountersIfEnabled();
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Handles the state change of an item in the collection.
    /// </summary>
    /// <param name="item">The item whose state has changed.</param>
    private void HandleItemStateChange(IStswTrackableItem item)
    {
        if (!TrackItems || item is not T typedItem)
            return;

        _addedItems.Remove(typedItem);
        _modifiedItems.Remove(typedItem);
        _deletedItems.Remove(typedItem);

        switch (item.ItemState)
        {
            case StswItemState.Added:
                _addedItems.Add(typedItem);
                break;
            case StswItemState.Modified:
                _modifiedItems.Add(typedItem);
                break;
            case StswItemState.Deleted:
                _deletedItems.Add(typedItem);
                break;
        }

        UpdateCountersIfEnabled();
    }

    /// <summary>
    /// Event handler for property changes of items in the collection.
    /// </summary>
    /// <param name="sender">The item that raised the event.</param>
    /// <param name="e">PropertyChangedEventArgs details.</param>
    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!TrackItems)
            return;

        if (sender is not IStswTrackableItem trackableItem)
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
    private void RebuildTrackingListsFromStates()
    {
        _addedItems.Clear();
        _modifiedItems.Clear();
        _deletedItems.Clear();

        if (!TrackItems)
            return;

        foreach (var item in this.OfType<IStswTrackableItem>())
        {
            if (item is not T typedItem)
                continue;

            switch (item.ItemState)
            {
                case StswItemState.Added:
                    _addedItems.Add(typedItem);
                    break;
                case StswItemState.Modified:
                    _modifiedItems.Add(typedItem);
                    break;
                case StswItemState.Deleted:
                    _deletedItems.Add(typedItem);
                    break;
            }
        }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Controls whether the collection tracks item state changes and mutates <see cref="IStswTrackableItem.ItemState"/>.
    /// When set to false, the collection behaves like a plain <see cref="ObservableCollection{T}"/> with no item-state updates and no tracking lists.
    /// Toggling this property re-wires subscriptions and rebuilds tracking lists/counters accordingly.
    /// </summary>
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
                foreach (var item in this.OfType<IStswTrackableItem>())
                    item.PropertyChanged += OnItemPropertyChanged;

                RebuildTrackingListsFromStates();
            }
            else
            {
                foreach (var item in this.OfType<IStswTrackableItem>())
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
    public IEnumerable<T> UnchangedItems => this.OfType<IStswTrackableItem>().Where(x => x.ItemState == StswItemState.Unchanged).OfType<T>();

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
        nameof(IStswTrackableItem.ShowDetails),
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

