using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// An async command implementation (with parameter) that can be used to bind to UI controls asynchronously with <see cref="Task"/> in order to execute a given action when triggered.
/// </summary>
/// <typeparam name="T">Parameter's type.</typeparam>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is <see langword="null"/>.</param>
public class StswAsyncCommand<T>(Func<T, Task> execute, Func<bool>? canExecute = null) : StswObservableObject, IStswAsyncCommand
{
    private readonly Func<T, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
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
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.</returns>
    public bool CanExecute(object? parameter = null) => (_canExecute?.Invoke() ?? true) && (!IsBusy || IsReusable);

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
    public async void Execute(object? parameter = null)
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
    private Task ExecuteAsync(T parameter)
    {
        return _execute(parameter);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the command can be reused while it is still executing.
    /// </summary>
    public bool IsReusable;

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
    /// Gets or sets the minimum possible value of the command execution.
    /// When both <see cref="Minimum"/> and <see cref="Maximum"/> properties are set to <c>0</c>,
    /// then progress ring in <see cref="StswProgressLabel"/> will become indeterminate.
    /// </summary>
    public double Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }
    private double _minimum = 0;

    /// <summary>
    /// Gets or sets the maximum possible value of the command execution.
    /// When both <see cref="Minimum"/> and <see cref="Maximum"/> properties are set to <c>0</c>,
    /// then progress ring in <see cref="StswProgressLabel"/> will become indeterminate.
    /// </summary>
    public double Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }
    private double _maximum = 100;

    /// <summary>
    /// Gets or sets the current state of the command.
    /// </summary>
    public StswProgressState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }
    private StswProgressState _state;

    /// <summary>
    /// Gets or sets the current magnitude of the command execution.
    /// </summary>
    public double Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
    private double _value;
}

/// <summary>
/// An async command implementation (without parameter) that can be used to bind to UI controls asynchronously with <see cref="Task"/> in order to execute a given action when triggered.
/// </summary>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is <see langword="null"/>.</param>
public class StswAsyncCommand(Func<Task> execute, Func<bool>? canExecute = null) : StswAsyncCommand<object>(_ => execute(), canExecute);

/* usage:

public StswAsyncCommand<string> RefreshCommand { get; }

public MainViewModel()
{
    RefreshCommand = new(Refresh, () => SomeCondition);
}

private async Task Refresh(string? parameter)
{
    // some action here
}

*/
