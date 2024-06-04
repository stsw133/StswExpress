using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StswExpress;

/// <summary>
/// Provides a base implementation of the <see cref="INotifyPropertyChanged"/> interface to enable objects to send notifications to clients when the value of a property changes.
/// </summary>
public abstract class StswObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event with the property name.
    /// </summary>
    /// <param name="propertyName">Name of the property (used to notify about different property than the one that has been set).</param>
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Sets the property value and raises the PropertyChanged event if the new value is different from the current value.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;

        NotifyPropertyChanged(propertyName);
        return true;
    }
}
