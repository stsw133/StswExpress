using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// An async command implementation (with parameter) that can be used to bind to UI controls asynchronously with Task in order to execute a given action when triggered.
/// </summary>
/// <typeparam name="T">Parameter's type.</typeparam>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="onCancel">The action to execute when the command is canceled.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is null.</param>
public class StswCancellableAsyncCommand<T>(Func<T?, CancellationToken, Task> execute, Action? onCancel = null, Func<bool>? canExecute = null) : StswObservableObject, IStswCommand
{
    private readonly Func<T?, CancellationToken, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Action? _onCancel = onCancel;
    private readonly Func<bool>? _canExecute = canExecute;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _executionTask;

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event.
    /// </summary>
    public void UpdateCanExecute() => Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns><see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.</returns>
    public bool CanExecute(object? parameter = null) => (_canExecute?.Invoke() ?? true) && (!IsBusy || IsReusable);

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    public async void Execute(object? parameter = null)
    {
        if (!CanExecute(parameter))
            return;

        _cancellationTokenSource = new CancellationTokenSource();
        IsBusy = true;
        UpdateCanExecute();

        _executionTask = ExecuteAsync((T?)parameter, _cancellationTokenSource.Token);

        try
        {
            await _executionTask;
        }
        catch (OperationCanceledException)
        {
            _onCancel?.Invoke();
        }
        finally
        {
            IsBusy = false;
            UpdateCanExecute();
            _cancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Asynchronously executes the command with the specified parameter and cancellation token.
    /// </summary>
    /// <param name="parameter">The parameter to be passed to the command execution logic.</param>
    /// <param name="token">A cancellation token that can be used to cancel the execution of the command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteAsync(T? parameter, CancellationToken token)
    {
        await _execute(parameter, token);
    }

    /// <summary>
    /// Cancels the ongoing operation if it is in progress.
    /// </summary>
    public async Task CancelAndWaitAsync()
    {
        if (IsBusy && _cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            if (_executionTask != null)
                await _executionTask;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the command is currently executing.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;

    /// <summary>
    /// Gets or sets a value indicating whether the command can be reused while it is still executing.
    /// </summary>
    public bool IsReusable;
}

/// <summary>
/// An async command implementation (without parameter) that can be used to bind to UI controls asynchronously with Task in order to execute a given action when triggered.
/// </summary>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="onCancel">The action to execute when the command is canceled.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is null.</param>
public class StswCancellableAsyncCommand(Func<CancellationToken, Task> execute, Action? onCancel = null, Func<bool>? canExecute = null)
    : StswCancellableAsyncCommand<object>((_, token) => execute(token), onCancel, canExecute)
{
}
