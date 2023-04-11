using System;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// A command implementation (without parameter) that can be used to bind to UI controls in order to execute a given action when triggered.
/// </summary>
public class StswRelayCommand : ICommand
{
    private readonly Action execute;
    private readonly Func<bool>? canExecute;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public StswRelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => canExecute == null || canExecute();
    public void Execute(object? parameter) => execute();
}

/// <summary>
/// A command implementation (with parameter) that can be used to bind to UI controls in order to execute a given action when triggered.
/// </summary>
/// <typeparam name="T">Parameter's type.</typeparam>
public class StswRelayCommand<T> : ICommand
{
    private readonly Action<T?> execute;
    private readonly Func<bool>? canExecute;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public StswRelayCommand(Action<T?> execute, Func<bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => canExecute == null || canExecute();
    public void Execute(object? parameter) => execute((T?)parameter);
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
