using System;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// A command implementation (with parameter) that can be used to bind to UI controls in order to execute a given action when triggered.
/// </summary>
/// <typeparam name="T">Parameter's type.</typeparam>
/// <param name="execute">The action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is <see langword="null"/>.</param>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// public StswCommand&lt;string&gt; SaveCommand { get; }
/// 
/// public MainViewModel()
/// {
///     SaveCommand = new(Save, () => SomeCondition);
/// }
/// 
/// private void Save(string? parameter)
/// {
///     // some action here
/// }
/// </code>
/// </example>
[StswInfo("0.1.0")]
public class StswCommand<T>(Action<T> execute, Func<bool>? canExecute = null) : StswObservableObject, ICommand
{
    private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
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
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.</returns>
    public bool CanExecute(object? parameter = null) => _canExecute?.Invoke() ?? true;

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
    public void Execute(object? parameter = null)
    {
        if (!CanExecute(parameter))
            return;

        _execute(parameter is T validParameter ? validParameter : default!);
    }
}

/// <summary>
/// A command implementation (without parameter) that can be used to bind to UI controls asynchronously with Task in order to execute a given action when triggered.
/// </summary>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is <see langword="null"/>.</param>
[StswInfo(null)]
public class StswCommand(Action execute, Func<bool>? canExecute = null) : StswCommand<object>(_ => execute(), canExecute);
