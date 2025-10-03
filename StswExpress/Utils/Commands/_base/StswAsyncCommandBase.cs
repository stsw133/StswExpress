using System;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// An abstract base class for asynchronous commands that can be used to bind to UI controls asynchronously.
/// </summary>
public abstract class StswAsyncCommandBase : StswObservableObject, ICommand
{
    /// <summary>
    /// Gets or sets a value indicating whether the command is currently executing.
    /// </summary>
    public virtual bool IsBusy
    {
        get => _isBusy;
        protected set => SetProperty(ref _isBusy, value);
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
    public abstract bool CanExecute(object? parameter);

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
    public abstract void Execute(object? parameter);
}
