using System.Collections.ObjectModel;

namespace StswExpress.Commons;

/// <summary>
/// Manages a collection of tasks, allowing for their execution and status tracking.
/// </summary>
/// <remarks>
/// This class is ideal for mixed task collections that do not necessarily return results.
/// It supports sequential and parallel execution, stop requests, and cancellation.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the manager with mixed tasks:
/// <code>
/// var manager = new StswTaskManager();
///
/// // add different types of tasks
/// manager.Add(() => Console.WriteLine("Task #1"));
/// manager.Add(async () => { await Task.Delay(200); Console.WriteLine("Task #2"); });
/// manager.Add(async ct => { await Task.Delay(500, ct); Console.WriteLine("Task #3"); });
///
/// // add an already constructed StswTask
/// manager.Add(StswTask.From(() => Console.WriteLine("Prebuilt task"), "Prebuilt"));
///
/// // run all tasks in parallel with a limit of 4 concurrent executions
/// await manager.RunAllParallelAsync(continueOnError: true, degreeOfParallelism: 4);
///
/// // cancel or stop remaining tasks if needed
/// manager.Stop();
/// manager.CancelAll();
/// </code>
/// </example>
public class StswTaskManager
{
    private bool _stopRequested;

    /// <summary>
    /// The collection of tasks managed by this manager. This collection is observable, allowing for UI updates when tasks are added or their status changes.
    /// </summary>
    public ObservableCollection<StswTaskBase> Tasks { get; } = [];

    /// <summary>
    /// Event that is raised when all tasks in the manager have completed execution.
    /// </summary>
    public Action? OnAllTasksCompleted { get; set; }

    /// <summary>
    /// Adds a new task to the manager.
    /// </summary>
    /// <param name="task">The task to be added to the manager.</param>
    public void Add(StswTaskBase task) => Tasks.Add(task);

    // Syntactic sugar:
    public StswTask Add(Action action, string? name = null) => AddInternal(StswTask.From(action, name));
    public StswTask Add(Func<Task> func, string? name = null) => AddInternal(StswTask.From(func, name));
    public StswTask Add(Func<CancellationToken, Task> func, string? name = null) => AddInternal(StswTask.From(func, name));
    public Task AddAndRun(Action action, string? name = null) => AddInternal(StswTask.From(action, name)).StartAsync();
    public Task AddAndRun(Func<Task> func, string? name = null) => AddInternal(StswTask.From(func, name)).StartAsync();
    public Task AddAndRun(Func<CancellationToken, Task> func, string? name = null) => AddInternal(StswTask.From(func, name)).StartAsync();

    /// <summary>
    /// Adds a range of tasks to the manager.
    /// </summary>
    /// <param name="tasks">The collection of tasks to be added to the manager.</param>
    public void AddRange(IEnumerable<StswTaskBase> tasks) { foreach (var t in tasks) Add(t); }

    /// <summary>
    /// Internal helper method to add a task to the manager and return it for further configuration if needed.
    /// </summary>
    /// <typeparam name="TTask">The type of the task being added, which must derive from <see cref="StswTaskBase"/>.</typeparam>
    /// <param name="task">The task instance to be added to the manager.</param>
    /// <returns>The same task instance that was added, allowing for method chaining or further configuration.</returns>
    private TTask AddInternal<TTask>(TTask task) where TTask : StswTaskBase
    {
        Tasks.Add(task);
        return task;
    }

    /// <summary>
    /// Runs all tasks that can be started, either in parallel or sequentially, based on the provided parameters.
    /// </summary>
    /// <param name="parallel">If <see langword="true"/>, tasks will be run in parallel; otherwise, they will be run sequentially.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of tasks to run in parallel. If set to 0 or less, it defaults to the number of processors.</param>
    /// <param name="continueOnError">If <see langword="true"/>, the manager will continue executing other tasks even if one fails.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync(bool parallel = true, int maxDegreeOfParallelism = 0, bool continueOnError = true)
    {
        _stopRequested = false;

        var pending = Tasks.Where(x => x.CanStart).ToList();
        if (pending.Count == 0)
        {
            RaiseAllTasksCompletedIfFinished();
            return;
        }

        if (!parallel || maxDegreeOfParallelism <= 1)
        {
            foreach (var t in pending)
            {
                if (_stopRequested) break;
                try { await t.StartAsync(); }
                catch when (continueOnError) { /* ignore */ }
            }
        }
        else
        {
            var dop = maxDegreeOfParallelism <= 0 ? Environment.ProcessorCount : maxDegreeOfParallelism;
            using var sem = new SemaphoreSlim(dop);
            var running = pending.Select(async t =>
            {
                await sem.WaitAsync();
                try
                {
                    if (!_stopRequested) await t.StartAsync();
                }
                catch when (continueOnError) { /* ignore */ }
                finally { sem.Release(); }
            });
            await Task.WhenAll(running);
        }

        RaiseAllTasksCompletedIfFinished();
    }

    // Syntactic sugar:
    public Task RunAllQueuedAsync(bool continueOnError) => RunAsync(parallel: false, maxDegreeOfParallelism: 1, continueOnError: continueOnError);
    public Task RunAllParallelAsync(bool continueOnError, int? degreeOfParallelism = null) => RunAsync(parallel: true, maxDegreeOfParallelism: degreeOfParallelism ?? Environment.ProcessorCount, continueOnError: continueOnError);

    /// <summary>
    /// Requests the manager to stop processing tasks. This will not cancel currently running tasks, but will prevent new tasks from starting.
    /// </summary>
    public void Stop() => _stopRequested = true;

    /// <summary>
    /// Cancels all running tasks managed by this manager.
    /// </summary>
    public void CancelAll()
    {
        foreach (var t in Tasks)
            t.Cancel();
    }

    /// <summary>
    /// Resets all tasks that cannot be started (i.e., those that are completed, faulted, or cancelled) back to their initial state, allowing them to be restarted.
    /// </summary>
    public void ResetFinished()
    {
        foreach (var t in Tasks.Where(x => !x.CanStart))
            t.Reset();
    }

    /// <summary>
    /// Awaits all tasks that are currently in the "Running" state to complete.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AwaitAllStartedAsync()
        => Task.WhenAll(Tasks.Where(t => t is StswTask st && st.Status == StswTaskStatus.Running
                                      || t is StswTask<object> st2 && st2.Status == StswTaskStatus.Running)
                          .Select(_ => Task.CompletedTask));

    /// <summary>
    /// Raises the <see cref="OnAllTasksCompleted"/> event if all tasks in the manager have completed execution.
    /// </summary>
    private void RaiseAllTasksCompletedIfFinished()
    {
        if (Tasks.Count > 0 &&
            Tasks.All(x => x.Status.In(StswTaskStatus.Completed, StswTaskStatus.Faulted, StswTaskStatus.Cancelled)))
            OnAllTasksCompleted?.Invoke();
    }
}

/// <summary>
/// A generic version of <see cref="StswTaskManager"/> that manages tasks returning results of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of result produced by the tasks managed by this manager.</typeparam>
/// <example>
/// The following example demonstrates how to use the generic task manager:
/// <code>
/// var manager = new StswTaskManager&lt;string&gt;();
///
/// manager.Add(async () => { await Task.Delay(100); return "Alpha"; });
/// manager.Add(async () => { await Task.Delay(200); return "Beta"; });
/// manager.Add(() => "Gamma");
///
/// // run all tasks concurrently
/// await manager.RunAllParallelAsync(continueOnError: false);
///
/// // read results
/// foreach (var res in manager.Results)
///     Console.WriteLine(res);
///
/// // add and immediately start a new one
/// await manager.AddAndRun(async () => { await Task.Delay(100); return "Delta"; });
/// </code>
/// </example>
public sealed class StswTaskManager<T> : StswTaskManager
{
    public StswTask<T> Add(Func<T> func, string? name = null) => AddInternal(StswTask<T>.From(func, name));
    public StswTask<T> Add(Func<Task<T>> func, string? name = null) => AddInternal(StswTask<T>.From(func, name));
    public StswTask<T> Add(Func<CancellationToken, Task<T>> func, string? name = null) => AddInternal(StswTask<T>.From(func, name));

    public Task<StswTask<T>> AddAndRun(Func<T> func, string? name = null) => AddAndRunInternal(StswTask<T>.From(func, name));
    public Task<StswTask<T>> AddAndRun(Func<Task<T>> func, string? name = null) => AddAndRunInternal(StswTask<T>.From(func, name));
    public Task<StswTask<T>> AddAndRun(Func<CancellationToken, Task<T>> func, string? name = null) => AddAndRunInternal(StswTask<T>.From(func, name));

    /// <summary>
    /// Gets a read-only list of results from all tasks managed by this manager. If a task has not completed or has failed, its result will be <see langword="null"/>.
    /// </summary>
    public IReadOnlyList<T?> Results => [.. Tasks.OfType<StswTask<T>>().Select(t => t.Result)];

    /// <summary>
    /// Internal helper method to add a task to the manager and return it for further configuration if needed.
    /// </summary>
    /// <typeparam name="TTask">The type of the task being added, which must derive from <see cref="StswTaskBase"/>.</typeparam>
    /// <param name="task">The task instance to be added to the manager.</param>
    /// <returns>The same task instance that was added, allowing for method chaining or further configuration.</returns>
    private TTask AddInternal<TTask>(TTask task) where TTask : StswTaskBase
    {
        base.Add(task);
        return task;
    }

    /// <summary>
    /// Adds a new task to the manager and starts it immediately.
    /// </summary>
    /// <param name="task">The task to be added and started.</param>
    /// <returns>A task representing the asynchronous operation, containing the started task.</returns>
    private async Task<StswTask<T>> AddAndRunInternal(StswTask<T> task)
    {
        base.Add(task);
        await task.StartAsync();
        return task;
    }
}