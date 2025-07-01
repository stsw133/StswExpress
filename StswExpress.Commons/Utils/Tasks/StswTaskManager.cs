using System.Collections.ObjectModel;

namespace StswExpress.Commons;

/// <summary>
/// Manages a collection of tasks, allowing for their execution and status tracking.
/// </summary>
/// <typeparam name="T">The type of the result produced by the tasks managed by this manager.</typeparam>
public class StswTaskManager<T>
{
    private bool _stopRequested = false;

    /// <summary>
    /// The collection of tasks managed by this manager. This collection is observable, allowing for UI updates when tasks are added or their status changes.
    /// </summary>
    public ObservableCollection<StswTask<T>> Tasks { get; } = [];

    #region Methods
    /// <summary>
    /// Event that is raised when all tasks in the manager have completed execution.
    /// </summary>
    public Action? OnAllTasksCompleted { get; set; }

    /// <summary>
    /// Adds a new task to the manager using a factory function that returns a <see cref="Task"/> of type <see cref="T"/>.
    /// </summary>
    /// <param name="taskFactory">A function that returns a <see cref="Task"/> of type <see cref="T"/>. This function will be invoked when the task is started.</param>
    public void Add(Func<Task<T>> taskFactory) => Tasks.Add(new StswTask<T>(taskFactory));

    /// <summary>
    /// Adds a new task to the manager using a factory function that takes a <see cref="CancellationToken"/> and returns a <see cref="Task"/> of type <see cref="T"/>.
    /// </summary>
    /// <param name="taskFactory">A function that takes a <see cref="CancellationToken"/> and returns a <see cref="Task"/> of type <see cref="T"/>. This function will be invoked when the task is started.</param>
    public void Add(Func<CancellationToken, Task<T>> taskFactory) => Tasks.Add(new StswTask<T>(taskFactory));

    /// <summary>
    /// Adds new tasks to the manager using a collection of factory functions that return a <see cref="Task"/> of type <see cref="T"/>.
    /// </summary>
    /// <param name="taskFactories">A collection of functions that return a <see cref="Task"/> of type <see cref="T"/>. Each function will be invoked when the corresponding task is started.</param>
    public void AddRange(IEnumerable<Func<Task<T>>> taskFactories)
    {
        foreach (var tf in taskFactories)
            Add(tf);
    }

    /// <summary>
    /// Adds new tasks to the manager using a collection of factory functions that take a <see cref="CancellationToken"/> and return a <see cref="Task"/> of type <see cref="T"/>.
    /// </summary>
    /// <param name="taskFactories">A collection of functions that take a <see cref="CancellationToken"/> and return a <see cref="Task"/> of type <see cref="T"/>. Each function will be invoked when the corresponding task is started.</param>
    public void AddRange(IEnumerable<Func<CancellationToken, Task<T>>> taskFactories)
    {
        foreach (var tf in taskFactories)
            Add(tf);
    }

    /// <summary>
    /// Raises the <see cref="OnAllTasksCompleted"/> event if all tasks in the manager have completed execution.
    /// </summary>
    private void RaiseAllTasksCompletedIfFinished()
    {
        if (Tasks.All(x => x.Status.In(StswTaskStatus.Completed, StswTaskStatus.Faulted, StswTaskStatus.Cancelled)))
            OnAllTasksCompleted?.Invoke();
    }

    /// <summary>
    /// Runs all queued tasks sequentially. If <paramref name="continueOnError"/> is set to <see langword="true"/>, it will continue executing other tasks even if one fails.
    /// </summary>
    /// <param name="continueOnError">If <see langword="true"/>, the task will continue execution even if an error occurs.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAllQueuedAsync(bool continueOnError)
    {
        _stopRequested = false;

        foreach (var task in Tasks.Where(x => x.Status == StswTaskStatus.Pending))
        {
            if (_stopRequested)
                break;

            try
            {
                await task.StartAsync();
            }
            catch when (continueOnError)
            {
                // ignore
            }
        }

        RaiseAllTasksCompletedIfFinished();
    }

    /// <summary>
    /// Runs all tasks in parallel. If <paramref name="continueOnError"/> is set to <see langword="true"/>, it will continue executing other tasks even if one fails.
    /// </summary>
    /// <param name="continueOnError">If <see langword="true"/>, the manager will continue executing other tasks even if one fails.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAllParallelAsync(bool continueOnError)
    {
        _stopRequested = false;

        var tasksToRun = Tasks.Where(x => x.Status == StswTaskStatus.Pending).ToList();
        var runningTasks = new List<Task>();

        foreach (var task in tasksToRun)
        {
            var runTask = task.StartAsync();
            if (!continueOnError)
                runningTasks.Add(runTask);
            else
                runningTasks.Add(runTask.ContinueWith(_ => { }));
        }

        await Task.WhenAll(runningTasks);
        RaiseAllTasksCompletedIfFinished();
    }

    /// <summary>
    /// Requests the manager to stop processing tasks. This will not cancel currently running tasks, but will prevent new tasks from starting.
    /// </summary>
    public void Stop()
    {
        _stopRequested = true;
    }
    #endregion
}

/* usage:

var manager = new StswTaskManager<int>();
manager.OnAllTasksCompleted = () => MessageBox.Show("All tasks completed!");

manager.Add(async () => { await Task.Delay(1000); return 42; }); // without token
manager.Add(async ct => { await Task.Delay(1000, ct); return 1337; }); // with token
manager.Add(async () => { await Task.Delay(500); throw new Exception("Error!"); });
manager.AddRange([
    async () => { await Task.Delay(100); return 1; },
    async () => { await Task.Delay(200); return 2; }
]);
manager.Tasks.Add(new StswTask<int>(async () => await Task.FromResult(5))
{
    Name = "Calculations #1",
    OnCompleted = task =>
    {
        if (task.Status == StswTaskStatus.Faulted)
            LogError(task.Exception);
    }
});

await manager.RunAllQueuedAsync();

*/
