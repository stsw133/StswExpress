using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// A command implementation (without parameter) that can be used to bind to UI controls in order to execute a given action when triggered.
/// </summary>
public sealed class StswCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public StswCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
}

/// <summary>
/// A command implementation (with parameter) that can be used to bind to UI controls in order to execute a given action when triggered.
/// </summary>
/// <typeparam name="T">Parameter's type.</typeparam>
public sealed class StswCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public StswCommand(Action<T?> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute((T?)parameter);
}

/// <summary>
/// An async command implementation (without parameter) that can be used to bind to UI controls asynchronously with Task in order to execute a given action when triggered.
/// </summary>
public class StswAsyncCommand : ICommand
{
    private Func<Task> _execute { get; }
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;
    public void UpdateCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    public bool IsWorking { get; private set; }

    public StswAsyncCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => !IsWorking && (_canExecute?.Invoke() ?? true);
    public async void Execute(object? parameter)
    {
        if (!IsWorking)
        {
            IsWorking = true;
            UpdateCanExecute();

            await _execute();

            IsWorking = false;
            UpdateCanExecute();
        }
    }
}

/// <summary>
/// An async command implementation (with parameter) that can be used to bind to UI controls asynchronously with Task in order to execute a given action when triggered.
/// </summary>
public class StswAsyncCommand<T> : ICommand
{
    private Func<object?, Task> _execute { get; }
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;
    public void UpdateCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    public bool IsWorking { get; private set; }

    public StswAsyncCommand(Func<object?, Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => !IsWorking && (_canExecute?.Invoke() ?? true);
    public async void Execute(object? parameter)
    {
        if (!IsWorking)
        {
            IsWorking = true;
            UpdateCanExecute();

            await _execute(parameter);

            IsWorking = false;
            UpdateCanExecute();
        }
    }
}

/// GlobalCommands:
/// 
/// Add         [Insert]
/// Archive     [Ctrl + I]
/// Clear       [F9]
/// Decrease    [-]
/// Delete
/// Duplicate   [Ctrl + Insert] [Ctrl + D]
/// Edit        [Ctrl + E]
/// Find        [Ctrl + F]
/// Fullscreen  [F11]
/// Help        [F1]
/// Increase    [+]
/// List        [F3]
/// New         [Ctrl + N]
/// Preview     [Ctrl + P]
/// Print       [F4]
/// Refresh     [F5]
/// Remove      [Delete]
/// Save        [Ctrl + S]
/// Select      [Ctrl + Space]
/// Settings    [F2]
