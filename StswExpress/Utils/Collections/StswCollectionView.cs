using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Provides a custom collection view for <see cref="StswBindingList{T}"/> that supports filtering, sorting, and grouping.
/// </summary>
/// <typeparam name="T">The type of elements in the collection, which must implement <see cref="IStswCollectionItem"/>.</typeparam>
[Obsolete]
public class StswCollectionView<T> : ICollectionView where T : IStswCollectionItem
{
    private readonly StswBindingList<T> _sourceCollection;
    private Predicate<object>? _filter;
    private List<T> _filteredItems;
    private ObservableCollection<GroupDescription> _groupDescriptions;
    private ReadOnlyObservableCollection<object>? _groups;
    private bool _deferRefresh;
    private bool _isRefreshDeferred;
    private bool _autoRefresh = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswCollectionView{T}"/> class.
    /// </summary>
    /// <param name="source">The source collection to view.</param>
    public StswCollectionView(StswBindingList<T> source)
    {
        _sourceCollection = source;
        _filteredItems = new(_sourceCollection);
        _groupDescriptions = [];
        _groups = new([]);

        _sourceCollection.ListChanged += OnSourceCollectionChanged;
        _groupDescriptions.CollectionChanged += OnGroupDescriptionsChanged;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the view should automatically refresh when changes occur in the collection.
    /// </summary>
    public bool AutoRefresh
    {
        get => _autoRefresh;
        set => _autoRefresh = value;
    }

    /// <summary>
    /// Gets or sets the predicate used to filter items in the collection.
    /// </summary>
    public Predicate<object>? Filter
    {
        get => _filter;
        set
        {
            _filter = value;
            if (_autoRefresh && !_isRefreshDeferred)
                Refresh();
        }
    }

    /// <summary>
    /// Gets a value indicating whether filtering is supported.
    /// </summary>
    public bool CanFilter => true;

    /// <summary>
    /// Gets a value indicating whether sorting is supported.
    /// </summary>
    public bool CanSort => true;

    /// <summary>
    /// Gets a value indicating whether grouping is supported.
    /// </summary>
    public bool CanGroup => true;

    /// <summary>
    /// Gets a value indicating whether the collection is empty after filtering.
    /// </summary>
    public bool IsEmpty => _filteredItems.Count == 0;

    /// <summary>
    /// Refreshes the view, applying current filters and groupings.
    /// </summary>
    public void Refresh()
    {
        if (_deferRefresh)
        {
            _isRefreshDeferred = true;
            return;
        }

        ApplyFilter();
        ApplyGrouping();
        OnCollectionChanged();
        _isRefreshDeferred = false;
    }

    /// <summary>
    /// Temporarily suspends automatic refresh operations until the returned object is disposed.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object that ends the defer cycle on disposal.</returns>
    public IDisposable DeferRefresh()
    {
        _deferRefresh = true;
        return new DeferHelper(() =>
        {
            _deferRefresh = false;
            Refresh();
        });
    }

    /// <summary>
    /// Gets the underlying collection.
    /// </summary>
    public IEnumerable SourceCollection => _sourceCollection;

    /// <summary>
    /// Gets a read-only collection of the current groups.
    /// </summary>
    public ReadOnlyObservableCollection<object>? Groups => _groups;

    /// <summary>
    /// Gets a collection of group descriptions used to group the items in the collection.
    /// </summary>
    public ObservableCollection<GroupDescription> GroupDescriptions => _groupDescriptions;

    /// <summary>
    /// Gets the current item in the view.
    /// </summary>
    public object? CurrentItem => _filteredItems.FirstOrDefault();

    /// <summary>
    /// Gets the current position of the item in the view.
    /// </summary>
    public int CurrentPosition => 0;

    /// <summary>
    /// Gets a value indicating whether the current item is beyond the end of the collection.
    /// </summary>
    public bool IsCurrentAfterLast => _filteredItems.Count == 0;

    /// <summary>
    /// Gets a value indicating whether the current item is before the beginning of the collection.
    /// </summary>
    public bool IsCurrentBeforeFirst => _filteredItems.Count == 0;

    /// <summary>
    /// Gets the collection of sort descriptions currently applied to the collection.
    /// </summary>
    public SortDescriptionCollection SortDescriptions => [];

    /// <summary>
    /// Gets or sets the cultural information for any operations of the collection view that may differ by culture, such as sorting.
    /// </summary>
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// Occurs when the collection view changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Occurs when the current item is about to change.
    /// </summary>
    public event CurrentChangingEventHandler? CurrentChanging;

    /// <summary>
    /// Occurs after the current item has changed.
    /// </summary>
    public event EventHandler? CurrentChanged;

    /// <summary>
    /// Determines whether the specified item is part of the view.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns><see langword="true"/> if the item is part of the view; otherwise, <see langword="false"/>.</returns>
    public bool Contains(object item) => _filteredItems.Contains((T)item);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    public IEnumerator GetEnumerator() => _filteredItems.GetEnumerator();

    /// <summary>
    /// Moves the current item to the specified object.
    /// </summary>
    /// <param name="item">The item to set as the current item.</param>
    /// <returns><see langword="true"/> if the current item was set successfully; otherwise, <see langword="false"/>.</returns>
    public bool MoveCurrentTo(object? item) => false;

    /// <summary>
    /// Moves the current item to the first item in the view.
    /// </summary>
    /// <returns><see langword="true"/> if the current item was set successfully; otherwise, <see langword="false"/>.</returns>
    public bool MoveCurrentToFirst() => false;

    /// <summary>
    /// Moves the current item to the last item in the view.
    /// </summary>
    /// <returns><see langword="true"/> if the current item was set successfully; otherwise, <see langword="false"/>.</returns>
    public bool MoveCurrentToLast() => false;

    /// <summary>
    /// Moves the current item to the next item in the view.
    /// </summary>
    /// <returns><see langword="true"/> if the current item was set successfully; otherwise, <see langword="false"/>.</returns>
    public bool MoveCurrentToNext() => false;

    /// <summary>
    /// Moves the current item to the item at the specified index.
    /// </summary>
    /// <param name="position">The index to set as the current item.</param>
    /// <returns><see langword="true"/> if the current item was set successfully; otherwise, <see langword="false"/>.</returns>
    public bool MoveCurrentToPosition(int position) => false;

    /// <summary>
    /// Moves the current item to the previous item in the view.
    /// </summary>
    /// <returns><see langword="true"/> if the current item was set successfully; otherwise, <see langword="false"/>.</returns>
    public bool MoveCurrentToPrevious() => false;

    /// <summary>
    /// Applies the current filter to the collection.
    /// </summary>
    private void ApplyFilter()
    {
        _filteredItems = _sourceCollection.Where(item => _filter?.Invoke(item) ?? true).ToList();
    }

    /// <summary>
    /// Applies the current group descriptions to the collection.
    /// </summary>
    private void ApplyGrouping()
    {
        if (_groupDescriptions.Count > 0)
        {
            var groupedItems = new ObservableCollection<object>();
            foreach (var groupDescription in _groupDescriptions)
            {
                var groups = _filteredItems.GroupBy(item => groupDescription.GroupNameFromItem(item, 0, CultureInfo.CurrentCulture))
                                           .Select(g => new StswCollectionViewGroup(g.Key, g.Cast<object>()));

                foreach (var group in groups)
                    groupedItems.Add(group);
            }
            _groups = new ReadOnlyObservableCollection<object>(groupedItems);
        }
        else
        {
            _groups = new ReadOnlyObservableCollection<object>([]);
        }
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event.
    /// </summary>
    protected void OnCollectionChanged()
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Handles changes in the source collection. Refreshes the view if auto-refresh is enabled
    /// and the refresh is not deferred.
    /// </summary>
    /// <param name="sender">The source of the event, typically the collection that was changed.</param>
    /// <param name="e">The event data containing information about the change.</param>
    private void OnSourceCollectionChanged(object? sender, ListChangedEventArgs e)
    {
        if (e.PropertyDescriptor?.Name?.In(IgnoredProperties) == true)
            return;

        if (_autoRefresh && !_isRefreshDeferred)
            Refresh();
    }

    /// <summary>
    /// Handles changes in the group descriptions. Refreshes the view if auto-refresh is enabled
    /// and the refresh is not deferred.
    /// </summary>
    /// <param name="sender">The source of the event, typically the collection of group descriptions.</param>
    /// <param name="e">The event data containing information about the change.</param>
    private void OnGroupDescriptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_autoRefresh && !_isRefreshDeferred)
            Refresh();
    }

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
    /// A helper class for deferring refresh operations on a collection view.
    /// Implements <see cref="IDisposable"/> to ensure refresh is resumed when disposed.
    /// </summary>
    private class DeferHelper : IDisposable
    {
        private readonly Action _onDispose;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferHelper"/> class with the specified action to execute upon disposal.
        /// </summary>
        /// <param name="onDispose">The action to execute when this instance is disposed, typically to resume refreshing.</param>
        public DeferHelper(Action onDispose)
        {
            _onDispose = onDispose;
        }

        /// <summary>
        /// Performs the action specified during initialization, typically to end the defer cycle and refresh the view.
        /// </summary>
        public void Dispose()
        {
            _onDispose();
        }
    }
}

/// <summary>
/// Represents a group of items in a collection view.
/// </summary>
public class StswCollectionViewGroup : CollectionViewGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StswCollectionViewGroup"/> class.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="items">The items to include in the group.</param>
    public StswCollectionViewGroup(object name, IEnumerable<object> items) : base(name)
    {
        foreach (var item in items)
            ProtectedItems.Add(item);
    }

    /// <summary>
    /// Gets a value indicating whether this group is at the bottom level (i.e., does not contain further subgroups).
    /// </summary>
    public override bool IsBottomLevel => true;
}
