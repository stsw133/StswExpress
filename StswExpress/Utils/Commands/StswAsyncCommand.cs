using System;
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
/// public StswAsyncCommand<string> RefreshCommand { get; }
/// 
/// public MainViewModel()
/// {
///     RefreshCommand = new(Refresh, () => SomeCondition);
/// }
/// 
/// private async Task Refresh(string? parameter)
/// {
///     // some action here
/// }
/// </code>
/// </example>
public class StswAsyncCommand<T>(Func<T, Task> execute, Func<bool>? canExecute = null) : StswAsyncCommandBase
{
    private readonly Func<T, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<bool>? _canExecute = canExecute;

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.</returns>
    public override bool CanExecute(object? parameter = null) => (_canExecute?.Invoke() ?? true) && (!IsBusy || IsReusable);

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
    public override async void Execute(object? parameter = null)
    {
        if (!CanExecute(parameter))
            return;

        IsBusy = true;
        UpdateCanExecute();

        try
        {
            await ExecuteAsync(parameter is T validParameter ? validParameter : default!);
        }
        finally
        {
            IsBusy = false;
            UpdateCanExecute();
        }
    }

    /// <summary>
    /// Asynchronously executes the command with the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to be passed to the command execution logic.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task ExecuteAsync(T parameter) => _execute(parameter);

    /// <summary>
    /// Gets or sets a value indicating whether the command can be reused while it is still executing.
    /// </summary>
    public bool IsReusable { get; set; } = false;
}

/// <summary>
/// An async command implementation (without parameter) that can be used to bind to UI controls asynchronously with <see cref="Task"/> in order to execute a given action when triggered.
/// </summary>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is <see langword="null"/>.</param>
public class StswAsyncCommand(Func<Task> execute, Func<bool>? canExecute = null) : StswAsyncCommand<object>(_ => execute(), canExecute);
