using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents an asynchronous command that can be paused and resumed.
/// </summary>
/// <typeparam name="T">The type of the parameter passed to the command.</typeparam>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is null.</param>
public class StswPausableAsyncCommand<T>(Func<T?, CancellationToken, Task> execute, Func<bool>? canExecute = null) : StswObservableObject, IStswCommand
{
    private readonly Func<T?, CancellationToken, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<bool>? _canExecute = canExecute;
    private CancellationTokenSource? _cancellationTokenSource;
    private int _currentIndex = 0;
    private IReadOnlyList<T>? _items;

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
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns><see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.</returns>
    public bool CanExecute(object? parameter = null) => (_canExecute?.Invoke() ?? true) && !IsBusy;

    /// <summary>
    /// Executes the command asynchronously with the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the command.</param>
    public async void Execute(object? parameter = null)
    {
        if (!CanExecute(parameter))
            return;

        _cancellationTokenSource = new CancellationTokenSource();
        IsBusy = true;
        UpdateCanExecute();

        try
        {
            await ExecuteAsync((T?)parameter, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // The command was paused
        }
        finally
        {
            IsBusy = false;
            UpdateCanExecute();
            _cancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Executes the asynchronous task with support for pausing and resuming.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the command.</param>
    /// <param name="token">The cancellation token used to pause or stop the task.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteAsync(T? parameter, CancellationToken token)
    {
        _items ??= new List<T>();

        for (; _currentIndex < _items.Count; _currentIndex++)
        {
            token.ThrowIfCancellationRequested();
            await _execute(_items[_currentIndex], token);
        }

        _currentIndex = 0;
        _items = null;
    }

    /// <summary>
    /// Pauses the command execution.
    /// </summary>
    public void Pause()
    {
        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Resumes the command execution from where it was paused.
    /// </summary>
    public void Resume()
    {
        if (IsBusy)
            return;

        Execute(null);
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
}

/// <summary>
/// An async command implementation (without parameter) that can be used to bind to UI controls asynchronously with Task in order to execute a given action when triggered.
/// </summary>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is null.</param>
public class StswPausableAsyncCommand(Func<CancellationToken, Task> execute, Func<bool>? canExecute = null)
    : StswPausableAsyncCommand<object>((_, token) => execute(token), canExecute);
