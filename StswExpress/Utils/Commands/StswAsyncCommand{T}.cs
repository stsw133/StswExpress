using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// An async command implementation (with parameter) that can be used to bind to UI controls asynchronously with Task in order to execute a given action when triggered.
/// </summary>
public class StswAsyncCommand<T> : StswObservableObject, ICommand
{
    private Func<object?, Task> _execute { get; }
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;
    public void UpdateCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public StswAsyncCommand(Func<object?, Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CanExecute(object? parameter) => !IsBusy && (_canExecute?.Invoke() ?? true);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    public async void Execute(object? parameter)
    {
        if (!IsBusy)
        {
            IsBusy = true;
            UpdateCanExecute();

            await _execute(parameter);

            IsBusy = false;
            UpdateCanExecute();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsBusy
    {
        get => isBusy;
        private set => SetProperty(ref isBusy, value);
    }
    private bool isBusy;
}
