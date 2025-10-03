namespace StswExpress.Commons;

/// <summary>
/// Represents a managed task with status tracking and exception handling.
/// </summary>
/// <typeparam name="T">The type of the result produced by the task.</typeparam>
[StswPlannedChanges(StswPlannedChanges.NewFeatures)]
public class StswTask<T> : StswObservableObject
{
    public StswTask(Func<Task<T>> taskFactory)
    {
        TaskFactory = taskFactory;
    }
    public StswTask(Func<CancellationToken, Task<T>> taskFactory)
    {
        _cts = new();
        TaskFactory = () => taskFactory(Token);
    }

    #region Properties
    /// <summary>
    /// Gets the factory method used to create tasks of type <see cref="Task{T}"/>.
    /// </summary>
    public Func<Task<T>> TaskFactory { get; }

    /// <summary>
    /// Gets the task instance associated with the current operation.
    /// </summary>
    public Task<T>? TaskInstance { get; private set; }

    /// <summary>
    /// Gets the unique identifier for the task instance, which can be used for tracking or logging purposes.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the task, which can be used for identification or logging purposes.
    /// </summary>
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string? _name;

    /// <summary>
    /// Gets the current status of the task.
    /// </summary>
    public StswTaskStatus Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }
    private StswTaskStatus _status = StswTaskStatus.Pending;

    /// <summary>
    /// Gets a cancellation token that can be used to cancel the task.
    /// </summary>
    public CancellationToken Token => _cts?.Token ?? CancellationToken.None;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Result produced by the task after successful execution.
    /// </summary>
    public T? Result
    {
        get => _result;
        private set => SetProperty(ref _result, value);
    }
    private T? _result;

    /// <summary>
    /// Gets the exception associated with the current operation, if any.
    /// </summary>
    public Exception? Exception
    {
        get => _exception;
        private set => SetProperty(ref _exception, value);
    }
    private Exception? _exception;

    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.Now;

    /// <summary>
    /// Gets the date and time when the operation started, or <see langword="null"/> if the operation has not started.
    /// </summary>
    public DateTime? StartedAt
    {
        get => _startedAt;
        private set => SetProperty(ref _startedAt, value);
    }
    private DateTime? _startedAt;

    /// <summary>
    /// Gets the date and time when the operation was completed, or <see langword="null"/> if the operation has not yet completed.
    /// </summary>
    public DateTime? CompletedAt
    {
        get => _completedAt;
        private set => SetProperty(ref _completedAt, value);
    }
    private DateTime? _completedAt;

    /// <summary>
    /// Gets a value indicating whether the task can be started.
    /// </summary>
    public bool CanStart => Status == StswTaskStatus.Pending;

    /// <summary>
    /// Event that is raised when the task has completed execution, either successfully or with an error.
    /// </summary>
    public Action<StswTask<T>>? OnCompleted { get; set; }
    #endregion

    #region Methods
    /// <summary>
    /// Starts the managed task asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync()
    {
        if (!CanStart)
            return;

        Status = StswTaskStatus.Running;
        StartedAt = DateTime.Now;

        try
        {
            TaskInstance = TaskFactory();
            Result = await TaskInstance;
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
    /// Resets the task to its initial state, allowing it to be reused.
    /// </summary>
    public void Reset()
    {
        if (Status.In(StswTaskStatus.Cancelled, StswTaskStatus.Faulted))
        {
            _cts = new();
            Status = StswTaskStatus.Pending;
            Exception = null;
            TaskInstance = null;
            Result = default;
            StartedAt = null;
            CompletedAt = null;
        }
    }
    #endregion
}
