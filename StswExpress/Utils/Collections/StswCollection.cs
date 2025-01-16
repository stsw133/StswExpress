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
/// <remarks>
/// Creates a new instance of <see cref="StswCollection{T}"/>.
/// </remarks>
/// <param name="ignoredPropertyNames">
/// A list of property names to ignore when checking modifications.
/// </param>
public class StswCollection<T> : ObservableCollection<T>, INotifyPropertyChanged where T : IStswCollectionItem
{
    private readonly IList<T> _addedItems = [];
    private readonly IList<T> _removedItems = [];
    private readonly IList<T> _modifiedItems = [];

    private int _countUnchanged;
    private int _countAdded;
    private int _countDeleted;
    private int _countModified;

    private readonly HashSet<string> _ignoredPropertyNames =
    [
        nameof(IStswCollectionItem.ShowDetails),
        nameof(IStswSelectionItem.IsSelected),
    ];

    /// <summary>
    /// Gets the collection of items that were added.
    /// </summary>
    public IReadOnlyList<T> AddedItems => [.. _addedItems];

    /// <summary>
    /// Gets the collection of items that were removed.
    /// </summary>
    public IReadOnlyList<T> RemovedItems => [.. _removedItems];

    /// <summary>
    /// Gets the collection of items that were modified.
    /// </summary>
    public IReadOnlyList<T> ModifiedItems => [.. _modifiedItems];

    /// <summary>
    /// Gets the number of items in the Unchanged state.
    /// </summary>
    public int CountUnchanged
    {
        get => _countUnchanged;
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
        get => _countAdded;
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
        get => _countDeleted;
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
        get => _countModified;
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
    /// 
    /// </summary>
    public HashSet<string> IgnoredPropertyNames => _ignoredPropertyNames;

    /// <summary>
    /// Creates a new instance of <see cref="StswCollection{T}"/>.
    /// </summary>
    public StswCollection() : base()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="TrackableObservableCollection{T}"/>.
    /// Elements added via Add() or InsertItem() will be marked as Added.
    /// </summary>
    /// <param name="ignoredPropertyNames">
    /// A list of property names to ignore when checking modifications.
    /// </param>
    public StswCollection(IEnumerable<string>? ignoredPropertyNames = null)
    {
        if (ignoredPropertyNames != null)
            foreach (var propertyName in ignoredPropertyNames)
                _ignoredPropertyNames.Add(propertyName);
    }

    /// <summary>
    /// Creates a new instance of <see cref="TrackableObservableCollection{T}"/>,
    /// initializing the collection with a set of items that will be treated as Unchanged.
    /// </summary>
    /// <param name="collection">Initial collection of items.</param>
    /// <param name="ignoredPropertyNames">
    /// A list of property names to ignore when checking modifications.
    /// </param>
    public StswCollection(IEnumerable<T> collection, IEnumerable<string>? ignoredPropertyNames = null)
    {
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

        IncrementCounter(item.ItemState);
    }

    /// <summary>
    /// Removes the item from the collection and marks it as Deleted,
    /// unless it was newly added (not yet accepted changes).
    /// </summary>
    /// <param name="index">Index of the item to remove.</param>
    protected override void RemoveItem(int index)
    {
        var item = this[index];
        base.RemoveItem(index); // remove this if want to show removed items in DataGrid

        // add this if want to show removed items in DataGrid
        /*
        var oldState = item.ItemState;
        if (oldState != StswItemState.Added)
        {
            DecrementCounter(oldState);
            item.ItemState = StswItemState.Deleted;
            _removedItems.Add(item);
            IncrementCounter(StswItemState.Deleted);
        }
        else
        {
            _addedItems.Remove(item);
            DecrementCounter(StswItemState.Added);
        }
        */

        // remove all below if want to show removed items in DataGrid
        var oldState = item.ItemState;

        item.PropertyChanged -= OnItemPropertyChanged;

        if (item.ItemState == StswItemState.Added && _addedItems.Contains(item))
        {
            _addedItems.Remove(item);
            DecrementCounter(StswItemState.Added);
        }
        else
        {
            if (item.ItemState != StswItemState.Deleted)
            {
                DecrementCounter(oldState);
                item.ItemState = StswItemState.Deleted;
                _removedItems.Add(item);
                IncrementCounter(StswItemState.Deleted);
            }

            _modifiedItems.Remove(item);
        }

        item.PropertyChanged -= OnItemPropertyChanged;
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

        IncrementCounter(item.ItemState);
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

        if (_ignoredPropertyNames.Contains(e.PropertyName!))
            return;

        if (trackableItem.ItemState == StswItemState.Added || trackableItem.ItemState == StswItemState.Deleted)
            return;

        if (trackableItem.ItemState == StswItemState.Unchanged)
        {
            trackableItem.ItemState = StswItemState.Modified;
            _modifiedItems.Add(trackableItem);
        }

        if (e.PropertyName == nameof(trackableItem.ItemState))
            RecountStates();
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
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Recounts the number of items in each state in the entire collection.
    /// Raises PropertyChanged for each count property.
    /// </summary>
    private void RecountStates()
    {
        var unchanged = 0;
        var added = 0;
        var modified = 0;
        //var deleted = 0;

        foreach (var item in this)
        {
            switch (item.ItemState)
            {
                case StswItemState.Unchanged:
                    unchanged++;
                    break;
                case StswItemState.Added:
                    added++;
                    break;
                case StswItemState.Modified:
                    modified++;
                    break;
                //case StswItemState.Deleted:
                //    deleted++;
                //    break;
            }
        }

        CountUnchanged = unchanged;
        CountAdded = added;
        CountModified = modified;
        //CountDeleted = deleted;
        CountDeleted = _removedItems.Count;
    }

    /// <summary>
    /// Increments the counter for the given state by 1.
    /// </summary>
    private void IncrementCounter(StswItemState state)
    {
        switch (state)
        {
            case StswItemState.Unchanged:
                CountUnchanged++;
                break;
            case StswItemState.Added:
                CountAdded++;
                break;
            case StswItemState.Deleted:
                CountDeleted++;
                break;
            case StswItemState.Modified:
                CountModified++;
                break;
        }
    }

    /// <summary>
    /// Decrements the counter for the given state by 1.
    /// </summary>
    private void DecrementCounter(StswItemState state)
    {
        switch (state)
        {
            case StswItemState.Unchanged:
                if (CountUnchanged > 0) CountUnchanged--;
                break;
            case StswItemState.Added:
                if (CountAdded > 0) CountAdded--;
                break;
            case StswItemState.Deleted:
                if (CountDeleted > 0) CountDeleted--;
                break;
            case StswItemState.Modified:
                if (CountModified > 0) CountModified--;
                break;
        }
    }
}

