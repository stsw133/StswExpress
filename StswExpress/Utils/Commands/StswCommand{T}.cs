using System;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// A command implementation (with parameter) that can be used to bind to UI controls in order to execute a given action when triggered.
/// </summary>
/// <typeparam name="T">Parameter's type.</typeparam>
public sealed class StswCommand<T> : StswObservableObject, ICommand
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

    /// <summary>
    /// 
    /// </summary>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    /// <summary>
    /// 
    /// </summary>
    public void Execute(object? parameter)
    {
        IsBusy = true;
        _execute((T?)parameter);
        IsBusy = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;
}
