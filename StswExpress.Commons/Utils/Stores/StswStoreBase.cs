namespace StswExpress.Commons;

/// <summary>
/// A generic base class for storing and managing a collection of models.
/// Provides functionality to load, add, remove, and refresh items in the store.
/// </summary>
/// <typeparam name="TModel">The type of the model being stored.</typeparam>
[Stsw("0.10.0", Changes = StswPlannedChanges.None)]
public class StswStoreBase<TModel>
{
    /// <summary>
    /// Event triggered when the store's contents change.
    /// </summary>
    public event EventHandler<StswStoreChangedArgs<TModel>>? StoreChanged;

    /// <summary>
    /// The list of items currently in the store.
    /// </summary>
    protected readonly List<TModel> _items = [];

    /// <summary>
    /// Indicates whether refresh events are currently deferred.
    /// </summary>
    protected bool _refreshDeferred;

    /// <summary>
    /// Lazy initialization of the store.
    /// </summary>
    private Lazy<Task> _initializeLazy;

    /// <summary>
    /// Gets the current collection of items in the store.
    /// </summary>
    public IEnumerable<TModel> Items => _items;

    /// <summary>
    /// Default constructor. Initializes the store without any specific fetch function.
    /// </summary>
    protected StswStoreBase()
    {
        _initializeLazy = new Lazy<Task>(Initialize);
    }

    /// <summary>
    /// Constructor that accepts a function for fetching items from a data source.
    /// </summary>
    /// <param name="fetchItemsFunc">Function to fetch items asynchronously.</param>
    protected StswStoreBase(Func<Task<IEnumerable<TModel>>> fetchItemsFunc)
    {
        FetchItemsFunc = fetchItemsFunc;
        _initializeLazy = new Lazy<Task>(Initialize);
    }

    /// <summary>
    /// Function used to fetch items from the data source.
    /// Can be overridden or set by derived classes.
    /// </summary>
    protected Func<Task<IEnumerable<TModel>>>? FetchItemsFunc { get; set; }

    /// <summary>
    /// Asynchronously loads the store's items by invoking the fetch function.
    /// </summary>
    public virtual async Task Load()
        => await _initializeLazy.Value;

    /// <summary>
    /// Fetches items from the database asynchronously.
    /// This method can be overridden in derived classes.
    /// </summary>
    /// <returns>A task that returns the collection of fetched items.</returns>
    protected virtual Task<IEnumerable<TModel>> FetchItemsFromDatabaseAsync()
        => FetchItemsFunc != null ? FetchItemsFunc() : Task.FromResult<IEnumerable<TModel>>([]);

    /// <summary>
    /// Initializes the store by clearing existing items and fetching new ones.
    /// </summary>
    private async Task Initialize()
    {
        using (DeferRefresh())
        {
            Clear();
            var items = await FetchItemsFromDatabaseAsync();
            _items.AddRange(items);
        }
    }

    /// <summary>
    /// Adds a new item to the store and triggers the store changed event.
    /// </summary>
    /// <param name="item">The item to add to the store.</param>
    public virtual void Add(TModel item)
    {
        _items.Add(item);
        OnStoreChanged();
    }

    /// <summary>
    /// Removes an item from the store and triggers the store changed event.
    /// </summary>
    /// <param name="item">The item to remove from the store.</param>
    public virtual void Remove(TModel item)
    {
        _items.Remove(item);
        OnStoreChanged();
    }

    /// <summary>
    /// Adds a range of items to the store and triggers the store changed event.
    /// </summary>
    /// <param name="items">The collection of items to add to the store.</param>
    public virtual void AddRange(List<TModel> items)
    {
        _items.AddRange(items);
        OnStoreChanged();
    }

    /// <summary>
    /// Clears all items from the store and triggers the store changed event.
    /// </summary>
    public virtual void Clear()
    {
        _items.Clear();
        OnStoreChanged();
    }

    /// <summary>
    /// Triggers the store changed event if refresh is not deferred.
    /// </summary>
    protected virtual void OnStoreChanged()
    {
        if (!_refreshDeferred)
            StoreChanged?.Invoke(this, new StswStoreChangedArgs<TModel>(_items));
    }

    /// <summary>
    /// Refreshes the store by clearing existing items and fetching new ones.
    /// </summary>
    public virtual void Refresh()
    {
        using (DeferRefresh())
        {
            Clear();
            var items = FetchItemsFromDatabaseAsync().Result;
            _items.AddRange(items);
        }
    }

    /// <summary>
    /// Defers the triggering of the store changed event until the deferred block is disposed.
    /// </summary>
    /// <returns>An IDisposable that ends the deferred block when disposed.</returns>
    public IDisposable DeferRefresh()
    {
        _refreshDeferred = true;
        return new StswRefreshBlocker(() =>
        {
            _refreshDeferred = false;
            OnStoreChanged();
        });
    }
}

/* usage:

public class ProjectsStore : StswStoreBase<ProjectModel>
{
    private readonly ISQL _sql;

    public ProjectsStore(ISQL sql) : base()
    {
        _sql = sql;
        FetchItemsFunc = async () => await Task.Run(() => _sql.GetProjects());
    }

    public override void AddItem(ProjectModel project)
    {
        base.Add(project);
    }

    public override void RemoveItem(ProjectModel project)
    {
        base.Remove(project);
    }

    public override void RefreshItems()
    {
        base.RefreshItems();
    }
}

*/
