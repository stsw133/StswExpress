using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Allows creating a trigger object for data binding purposes.
/// </summary>
public class StswBindingTrigger : INotifyPropertyChanged
{
    public StswBindingTrigger()
    {
        Binding = new Binding()
        {
            Source = this,
            Path = new PropertyPath(nameof(Value))
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public Binding Binding { get; }

    /// <summary>
    /// 
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 
    /// </summary>
    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

    /// <summary>
    /// 
    /// </summary>
    public object? Value { get; }
}
