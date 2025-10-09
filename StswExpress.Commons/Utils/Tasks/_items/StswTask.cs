namespace StswExpress.Commons;

/// <summary>
/// Represents a base class for tasks with status tracking and cancellation support.
/// </summary>
/// <example>
/// The following example demonstrates how to inherit from <see cref="StswTaskBase"/>:
/// <code>
/// public class CustomTask : StswTaskBase
/// {
///     protected override async Task RunCoreAsync(CancellationToken ct)
///     {
///         await Task.Delay(500, ct);
///         Console.WriteLine("Finished!");
///     }
/// }
///
/// var task = new CustomTask();
/// await task.StartAsync();
/// </code>
/// </example>
public abstract class StswTaskBase : StswObservableObject
{
    protected StswTaskBase()
    {
        Id = Guid.NewGuid();
        _cts = new();
    }

    /// <summary>
    /// Gets the unique identifier for the task instance, which can be used for tracking or logging purposes.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets or sets the name of the task, which can be used for identification or logging purposes.
    /// </summary>
    public string? Name { get => _name; set => SetProperty(ref _name, value); }
    private string? _name;

    /// <summary>
    /// Gets the current status of the task.
    /// </summary>
    public StswTaskStatus Status { get => _status; protected set => SetProperty(ref _status, value); }
    private StswTaskStatus _status = StswTaskStatus.Pending;

    /// <summary>
    /// Gets the exception associated with the task, if any.
    /// </summary>
    public Exception? Exception { get => _exception; protected set => SetProperty(ref _exception, value); }
    private Exception? _exception;

    /// <summary>
    /// Gets the date and time when the task was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.Now;

    /// <summary>
    /// Gets the date and time when the task started, or <see langword="null"/> if the task has not started.
    /// </summary>
    public DateTime? StartedAt { get => _startedAt; protected set => SetProperty(ref _startedAt, value); }
    private DateTime? _startedAt;

    /// <summary>
    /// Gets the date and time when the task completed, or <see langword="null"/> if the task has not yet completed.
    /// </summary>
    public DateTime? CompletedAt { get => _completedAt; protected set => SetProperty(ref _completedAt, value); }
    private DateTime? _completedAt;

    /// <summary>
    /// Gets a value indicating whether the task can be started.
    /// </summary>
    public bool CanStart => Status == StswTaskStatus.Pending;

    /// <summary>
    /// Event that is raised when the task has completed execution, either successfully or with an error.
    /// </summary>
    public Action<StswTaskBase>? OnCompleted { get; set; }

    /// <summary>
    /// Gets the cancellation token that can be used to cancel the task.
    /// </summary>
    public CancellationToken Token => _cts?.Token ?? CancellationToken.None;
    protected CancellationTokenSource? _cts;

    /// <summary>
    /// When overridden in a derived class, contains the core logic of the task to be executed asynchronously.
    /// </summary>
    /// <param name="ct">The cancellation token that can be used to cancel the task.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task RunCoreAsync(CancellationToken ct);

    /// <summary>
    /// Starts the task asynchronously if it is in a state that allows it to be started.
    /// </summary>
    /// <returns></returns>
    public async Task StartAsync()
    {
        if (!CanStart)
            return;

        Status = StswTaskStatus.Running;
        StartedAt = DateTime.Now;

        try
        {
            await RunCoreAsync(Token).ConfigureAwait(false);
            Status = Token.IsCancellationRequested ? StswTaskStatus.Cancelled : StswTaskStatus.Completed;
        }
        catch (OperationCanceledException)
        {
            Status = StswTaskStatus.Cancelled;
        }
        catch (Exception ex)
        {
            Exception = ex;
            Status = StswTaskStatus.Faulted;
        }
        finally
        {
            CompletedAt = DateTime.Now;
            OnCompleted?.Invoke(this);
        }
    }

    /// <summary>
    /// Cancels the managed task if it is currently running.
    /// </summary>
    public void Cancel()
    {
        if (_cts?.IsCancellationRequested == false)
        {
            _cts.Cancel();
            Status = StswTaskStatus.Cancelled;
            CompletedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Resets the task to its initial state, allowing it to be restarted.
    /// </summary>
    public virtual void Reset()
    {
        if (!Status.In(StswTaskStatus.Cancelled, StswTaskStatus.Faulted, StswTaskStatus.Completed))
            return;

        _cts?.Dispose();
        _cts = new();
        Status = StswTaskStatus.Pending;
        Exception = null;
        StartedAt = null;
        CompletedAt = null;
    }
}

/// <summary>
/// Represents a managed task with status tracking and exception handling.
/// </summary>
/// <example>
/// The following example demonstrates how to create and run several managed tasks:
/// <code>
/// var manager = new StswTaskManager();
///
/// // add synchronous, asynchronous, and cancellable tasks
/// manager.Add(() => Console.WriteLine("Synchronous work"));
/// manager.Add(async () => { await Task.Delay(200); Console.WriteLine("Async work"); });
/// manager.Add(async ct => { await Task.Delay(300, ct); Console.WriteLine("Cancellable work"); });
///
/// // add and start immediately
/// await manager.AddAndRun(() => Console.WriteLine("Immediate run"));
///
/// // run remaining tasks sequentially
/// await manager.RunAllQueuedAsync(continueOnError: true);
/// </code>
/// </example>
public sealed class StswTask : StswTaskBase
{
    private readonly Func<CancellationToken, Task> _factory;

    public StswTask(Action action) : this(_ => { action(); return Task.CompletedTask; }) { }
    public StswTask(Func<Task> asyncAction) : this(_ => asyncAction()) { }
    public StswTask(Func<CancellationToken, Task> factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <summary>
    /// When overridden in a derived class, contains the core logic of the task to be executed asynchronously.
    /// </summary>
    /// <param name="ct">The cancellation token that can be used to cancel the task.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override Task RunCoreAsync(CancellationToken ct) => _factory(ct);

    /// <summary>
    /// Restarts the task by resetting its state and starting it again.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RestartAsync() { Reset(); return StartAsync(); }

    // Syntactic sugar:
    public static StswTask From(Action action, string? name = null) => new(action) { Name = name };
    public static StswTask From(Func<Task> func, string? name = null) => new(func) { Name = name };
    public static StswTask From(Func<CancellationToken, Task> func, string? name = null) => new(func) { Name = name };
}

/// <summary>
/// Represents a managed task with status tracking, exception handling, and a result of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the result produced by the task.</typeparam>
/// <example>
/// The following example demonstrates how to create, run, and retrieve results:
/// <code>
/// var manager = new StswTaskManager&lt;int&gt;();
///
/// // add tasks returning values
/// manager.Add(() => 42, "Quick calc");
/// manager.Add(async () => { await Task.Delay(300); return 1337; }, "Delayed calc");
///
/// // add and run immediately
/// var running = await manager.AddAndRun(async () => { await Task.Delay(100); return 7; }, "Instant run");
///
/// // run remaining tasks sequentially
/// await manager.RunAllQueuedAsync(continueOnError: false);
///
/// // access results
/// foreach (var result in manager.Results)
///     Console.WriteLine(result);
/// </code>
/// </example>
public sealed class StswTask<T> : StswTaskBase
{
    private readonly Func<CancellationToken, Task<T>> _factory;

    public StswTask(Func<T> func) : this(_ => Task.FromResult(func())) { }
    public StswTask(Func<Task<T>> func) : this(_ => func()) { }
    public StswTask(Func<CancellationToken, Task<T>> factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <summary>
    /// Gets the task instance associated with the current operation.
    /// </summary>
    public Task<T>? TaskInstance { get; private set; }

    /// <summary>
    /// Gets the result of the task, or <see langword="null"/> if the task has not completed successfully.
    /// </summary>
    public T? Result { get => _result; private set => SetProperty(ref _result, value); }
    private T? _result;

    /// <summary>
    /// When overridden in a derived class, contains the core logic of the task to be executed asynchronously.
    /// </summary>
    /// <param name="ct">The cancellation token that can be used to cancel the task.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task RunCoreAsync(CancellationToken ct)
    {
        TaskInstance = _factory(ct);
        Result = await TaskInstance.ConfigureAwait(false);
    }

    /// <summary>
    /// Starts the task asynchronously and returns the result upon completion.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the result of type <typeparamref name="T"/>.</returns>
    public async Task<T?> StartAndGetAsync()
    {
        await StartAsync().ConfigureAwait(false);
        return Result;
    }

    /// <summary>
    /// Restarts the task by resetting its state and starting it again, returning the result upon completion.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the result of type <typeparamref name="T"/>.</returns>
    public Task<T?> RestartAndGetAsync() { Reset(); return StartAndGetAsync(); }

    /// <summary>
    /// Resets the task to its initial state, allowing it to be restarted.
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        TaskInstance = null;
        Result = default;
    }

    // Syntactic sugar:
    public static StswTask<T> From(Func<T> func, string? name = null) => new(func) { Name = name };
    public static StswTask<T> From(Func<Task<T>> func, string? name = null) => new(func) { Name = name };
    public static StswTask<T> From(Func<CancellationToken, Task<T>> func, string? name = null) => new(func) { Name = name };
}
