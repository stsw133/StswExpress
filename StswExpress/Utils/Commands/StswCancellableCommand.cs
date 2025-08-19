using System;
using System.Threading;
using System.Threading.Tasks;

namespace StswExpress;

/// <summary>
/// An async command implementation (with parameter) that can be used to bind to UI controls asynchronously with <see cref="Task"/> in order to execute a given action when triggered.
/// </summary>
/// <typeparam name="T">Parameter's type.</typeparam>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is <see langword="null"/>.</param>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// public StswCancellableCommand RefreshCommand { get; }
/// 
/// public MainViewModel()
/// {
///     RefreshCommand = new(Refresh, () => true);
/// }
/// 
/// private async Task Refresh(CancellationToken token)
/// {
///     StatusMessage = "Processing...";
///     Progress = 0;
/// 
///     for (int i = 0; i &lt;= 100; i++)
///     {
///         if (token.IsCancellationRequested)
///         {
///             StatusMessage = "Operation cancelled.";
///             return;
///         }
/// 
///         Progress = i;
///         await Task.Delay(50, token);
///     }
/// 
///     StatusMessage = "Finished!";
/// }
/// </code>
/// </example>
[StswInfo("0.9.2", "0.20.0")]
public class StswCancellableCommand<T>(Func<T, CancellationToken, Task> execute, Func<bool>? canExecute = null) : StswAsyncCommandBase
{
    private readonly Func<T, CancellationToken, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<bool>? _canExecute = canExecute;

    [StswInfo("0.20.0")]
    protected CancellationTokenSource? CancellationTokenSource
    {
        get => _cancellationTokenSource;
        set => _cancellationTokenSource = value;
    }
    private CancellationTokenSource? _cancellationTokenSource;

    [StswInfo("0.20.0")]
    protected Task? ExecutionTask
    {
        get => _executionTask;
        set => _executionTask = value;
    }
    private Task? _executionTask;

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.2", "0.20.0")]
    public override bool CanExecute(object? parameter = null) => IsBusy || (_canExecute?.Invoke() ?? true);

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
    [StswInfo("0.9.2", "0.20.0")]
    public async override void Execute(object? parameter = null)
    {
        if (IsBusy && CancellationTokenSource is not null)
        {
            CancellationTokenSource.Cancel();
            if (ExecutionTask is not null)
                await ExecutionTask;
            return;
        }

        CancellationTokenSource = new CancellationTokenSource();
        IsBusy = true;
        UpdateCanExecute();

        ExecutionTask = ExecuteAsync(parameter is T validParameter ? validParameter : default!, CancellationTokenSource.Token);

        try
        {
            await ExecutionTask;
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled
        }
        finally
        {
            IsBusy = false;
            UpdateCanExecute();
            CancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Asynchronously executes the command with the specified parameter and cancellation token.
    /// </summary>
    /// <param name="parameter">The parameter to be passed to the command execution logic.</param>
    /// <param name="token">A cancellation token that can be used to cancel the execution of the command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteAsync(T parameter, CancellationToken token) => await _execute(parameter, token);

    /// <summary>
    /// Cancels the ongoing operation if it is in progress.
    /// </summary>
    public async Task CancelAndWaitAsync()
    {
        if (IsBusy && CancellationTokenSource != null)
        {
            CancellationTokenSource.Cancel();
            if (ExecutionTask != null)
                await ExecutionTask;
        }
    }
}

/// <summary>
/// An async command implementation (without parameter) that can be used to bind to UI controls asynchronously with <see cref="Task"/> in order to execute a given action when triggered.
/// </summary>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is <see langword="null"/>.</param>
[StswInfo("0.9.2", "0.20.0")]
public class StswCancellableCommand(Func<CancellationToken, Task> execute, Func<bool>? canExecute = null)
    : StswCancellableCommand<object>((_, token) => execute(token), canExecute);
