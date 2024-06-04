using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// An async command implementation (with parameter) that can be used to bind to UI controls asynchronously with Task in order to execute a given action when triggered.
/// </summary>
/// <typeparam name="T">Parameter's type.</typeparam>
public class StswAsyncCommand<T> : StswObservableObject, ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;
    public void UpdateCanExecute() => Application.Current.Dispatcher.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));

    public StswAsyncCommand(Func<object?, Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CanExecute(object? parameter) => /*!IsBusy &&*/ (_canExecute?.Invoke() ?? true);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    public async void Execute(object? parameter)
    {
        if (!IsBusy || IsReusable)
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
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;

    /// <summary>
    /// 
    /// </summary>
    public bool IsReusable;
}
