using System;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// A command implementation (with parameter) that can be used to bind to UI controls in order to execute a given action when triggered.
/// </summary>
/// <typeparam name="T">Parameter's type.</typeparam>
/// <param name="execute">The action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is null.</param>
public class StswCommand<T>(Action<T?> execute, Func<bool>? canExecute = null) : StswObservableObject, ICommand
{
    private readonly Action<T?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<bool>? _canExecute = canExecute;

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
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    public void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        IsBusy = true;
        try
        {
            _execute((T?)parameter);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the command is currently executing.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;
}

/// <summary>
/// A command implementation (without parameter) that can be used to bind to UI controls asynchronously with Task in order to execute a given action when triggered.
/// </summary>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is null.</param>
public class StswCommand(Action execute, Func<bool>? canExecute = null) : StswCommand<object>(_ => execute(), canExecute)
{
}
