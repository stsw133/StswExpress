using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;
/// <summary>
/// Allows creating a trigger object for data binding purposes.
/// </summary>
[Stsw("0.3.0")]
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
    /// Gets the binding that is created for the trigger.
    /// </summary>
    public Binding Binding { get; }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Refreshes the binding by raising the PropertyChanged event for the Value property.
    /// </summary>
    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

    /// <summary>
    /// Gets the value of the binding trigger.
    /// </summary>
    public object? Value { get; }
}
