using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StswExpress;

/// <summary>
/// A custom <see cref="ObservableCollection{T}"/> that tracks changes on items and
/// maintains separate lists for added, removed, and modified items.
/// Also allows ignoring certain property names to prevent marking
/// items as modified when those properties change.
/// </summary>
/// <typeparam name="T">Item type implementing <see cref="IStswCollectionItem"/></typeparam>
public class StswObservableCollection<T> : ObservableCollection<T> where T : IStswCollectionItem
{
    //TODO - handle changes in nested classes

    private readonly IList<T> _addedItems = [];
    private readonly IList<T> _removedItems = [];
    private readonly IList<T> _modifiedItems = [];
    private readonly bool _showRemovedItems;

    /// <summary>
    /// Gets the collection of ignored property names when tracking modifications.
    /// </summary>
    public HashSet<string> IgnoredPropertyNames => _ignoredPropertyNames;
    private readonly HashSet<string> _ignoredPropertyNames =
    [
        nameof(IStswCollectionItem.ShowDetails),
        nameof(IStswSelectionItem.IsSelected),
    ];

    /// <summary>
    /// Initializes a new instance of <see cref="StswObservableCollection{T}"/>.
    /// Elements added via Add() or InsertItem() will be marked as Added.
    /// </summary>
    /// <param name="showRemovedItems">Indicates whether removed items should be tracked.</param>
    /// <param name="ignoredPropertyNames">A list of property names to ignore when checking modifications.</param>
    public StswObservableCollection(bool showRemovedItems = false, IEnumerable<string>? ignoredPropertyNames = null) : base()
    {
        _showRemovedItems = showRemovedItems;

        if (ignoredPropertyNames != null)
            foreach (var propertyName in ignoredPropertyNames)
                _ignoredPropertyNames.Add(propertyName);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StswObservableCollection{T}"/> with an initial set of items.
    /// Items are treated as Unchanged upon initialization.
    /// </summary>
    /// <param name="collection">Initial collection of items.</param>
    /// <param name="showRemovedItems">Indicates whether removed items should be tracked.</param>
    /// <param name="ignoredPropertyNames">A list of property names to ignore when checking modifications.</param>
    public StswObservableCollection(IEnumerable<T> collection, bool showRemovedItems = false, IEnumerable<string>? ignoredPropertyNames = null) : base()
    {
        _showRemovedItems = showRemovedItems;

        if (ignoredPropertyNames != null)
            foreach (var propertyName in ignoredPropertyNames)
                _ignoredPropertyNames.Add(propertyName);

        if (collection != null)
        {
            foreach (var item in collection)
            {
                item.ItemState = StswItemState.Unchanged;
                Items.Add(item);
                item.PropertyChanged += OnItemPropertyChanged;
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        RecountStates();
        foreach (var item in this)
            item.PropertyChanged += OnItemPropertyChanged;
    }

    /// <summary>
    /// Inserts an item into the collection. The item is marked as Added
    /// if it's in Unchanged state or previously marked as Deleted.
    /// </summary>
    /// <param name="index">Index at which the item should be inserted.</param>
    /// <param name="item">The item to be inserted.</param>
    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);
        
        item.PropertyChanged += OnItemPropertyChanged;

        switch (item.ItemState)
        {
            case StswItemState.Unchanged:
            case StswItemState.Deleted:
            case StswItemState.Modified:
                item.ItemState = StswItemState.Added;
                _addedItems.Add(item);
                break;

            case StswItemState.Added:
                _addedItems.AddIfNotContains(item);
                break;
        }

        UpdateCounter(item.ItemState, +1);
    }

    /// <summary>
    /// Removes the item from the collection and marks it as Deleted,
    /// unless it was newly added (not yet accepted changes).
    /// </summary>
    /// <param name="index">Index of the item to remove.</param>
    protected override void RemoveItem(int index)
    {
        var item = this[index];

        if (_showRemovedItems)
        {
            var oldState = item.ItemState;
            if (oldState != StswItemState.Added)
            {
                UpdateCounter(oldState, -1);
                item.ItemState = StswItemState.Deleted;
                _removedItems.Add(item);
                UpdateCounter(StswItemState.Deleted, +1);
            }
            else
            {
                _addedItems.Remove(item);
                UpdateCounter(StswItemState.Added, -1);
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
                UpdateCounter(StswItemState.Added, -1);
            }
            else
            {
                if (item.ItemState != StswItemState.Deleted)
                {
                    UpdateCounter(oldState, -1);
                    item.ItemState = StswItemState.Deleted;
                    _removedItems.Add(item);
                    UpdateCounter(StswItemState.Deleted, +1);
                }

                _modifiedItems.Remove(item);
            }
        }
    }

    /// <summary>
    /// Clears the collection, marking existing items as Deleted if they
    /// are not newly added.
    /// </summary>
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
                    _removedItems.Add(item);
                }

                _modifiedItems.Remove(item);
            }
        }

        base.ClearItems();

        CountUnchanged = 0;
        CountAdded = 0;
        CountDeleted = 0;
        CountModified = 0;
    }

    /// <summary>
    /// Sets an item at a specific index, marking the replaced item as Deleted
    /// (unless it was newly added) and the new item as Added.
    /// </summary>
    /// <param name="index">The index of the item to replace.</param>
    /// <param name="item">The new item.</param>
    protected override void SetItem(int index, T item)
    {
        var oldItem = this[index];
        RemoveItem(index);

        base.SetItem(index, item);
        item.PropertyChanged += OnItemPropertyChanged;

        if (item.ItemState.In(StswItemState.Unchanged, StswItemState.Deleted, StswItemState.Modified))
        {
            item.ItemState = StswItemState.Added;
            _addedItems.Add(item);
        }
        else
        {
            _addedItems.AddIfNotContains(item);
        }

        UpdateCounter(item.ItemState, +1);
    }

    /// <summary>
    /// Adds a range of items to the collection, marking them as Added unless they are already in that state.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        if (items.IsNullOrEmpty())
            return;

        List<T> added = [];
        foreach (var item in items)
        {
            if (item.ItemState.In(StswItemState.Unchanged, StswItemState.Deleted, StswItemState.Modified))
            {
                item.ItemState = StswItemState.Added;
                _addedItems.Add(item);
            }
            else
            {
                _addedItems.AddIfNotContains(item);
            }

            item.PropertyChanged += OnItemPropertyChanged;
            Items.Add(item);
            added.Add(item);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
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
            RecountStates();
            return;
        }

        if (_ignoredPropertyNames.Contains(e.PropertyName!))
            return;

        if (_countersAreDirty)
            return;

        if (trackableItem.ItemState == StswItemState.Added || trackableItem.ItemState == StswItemState.Deleted)
            return;

        if (trackableItem.ItemState == StswItemState.Unchanged)
        {
            trackableItem.ItemState = StswItemState.Modified;
            _modifiedItems.Add(trackableItem);
            UpdateCounter(StswItemState.Modified, +1);
        }
    }

    /// <summary>
    /// Marks all changes as accepted. This method resets the ItemState on
    /// items to Unchanged and clears internal tracking lists.
    /// </summary>
    public void AcceptChanges()
    {
        foreach (var item in _addedItems)
            item.ItemState = StswItemState.Unchanged;
        
        foreach (var item in _removedItems)
            item.ItemState = StswItemState.Unchanged;
        
        foreach (var item in _modifiedItems)
            item.ItemState = StswItemState.Unchanged;

        _addedItems.Clear();
        _removedItems.Clear();
        _modifiedItems.Clear();

        RecountStates();
    }

    /// <summary>
    /// Retrieves all items with a specific state.
    /// </summary>
    /// <param name="state">The state of the items to retrieve.</param>
    /// <returns>A collection of items with the specified state.</returns>
    public IEnumerable<T> GetItemsByState(StswItemState state) => state switch
    {
        StswItemState.Added => _addedItems,
        StswItemState.Deleted => _removedItems,
        StswItemState.Modified => _modifiedItems,
        StswItemState.Unchanged => this.Where(x => x.ItemState == StswItemState.Unchanged),
        _ => []
    };

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

    /// <<summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Recounts the number of items in each state in the entire collection.
    /// Raises PropertyChanged for each count property.
    /// </summary>
    private void RecountStates()
    {
        if (_countersAreDirty)
            return;

        CountUnchanged = this.Count(x => x.ItemState == StswItemState.Unchanged);
        CountAdded = _addedItems.Count;
        CountModified = _modifiedItems.Count;
        CountDeleted = _removedItems.Count;
    }

    /// <summary>
    /// Increments or decrements the counter for the given state by 1.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="delta"></param>
    private void UpdateCounter(StswItemState state, int delta)
    {
        if (_countersAreDirty)
            return;

        switch (state)
        {
            case StswItemState.Unchanged:
                CountUnchanged += delta;
                break;
            case StswItemState.Added:
                CountAdded += delta;
                break;
            case StswItemState.Deleted:
                CountDeleted += delta;
                break;
            case StswItemState.Modified:
                CountModified += delta;
                break;
        }
    }
    #endregion
}

