using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StswExpress.Commons;
/// <summary>
/// Provides a base implementation of the <see cref="INotifyPropertyChanged"/> interface to enable objects to send notifications to clients when the value of a property changes.
/// </summary>
public abstract class StswObservableObject : INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event with the property name.
    /// </summary>
    /// <param name="propertyName">Name of the property (used to notify about different property than the one that has been set).</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Sets the property value and raises the <see cref="PropertyChanged"/> event if the new value is different from the current value.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">Reference to the field storing the property's value.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyNamesToNotify">Collection of property names to notify in addition to the current property.</param>
    /// <param name="doAfter">The action to execute after the property value is changed.</param>
    /// <param name="condition">An optional condition that must be met for the property to be set. If this condition is not met, the property will not be set and no notification will be sent.</param>
    /// <param name="propertyName">The name of the property. This is optional and can be automatically provided by the compiler.</param>
    /// <returns><see langword="true"/> if the property value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T field, T value, IEnumerable<string>? propertyNamesToNotify = null, Action? doAfter = null, Func<bool>? condition = null, [CallerMemberName] string propertyName = "")
    {
        if (condition != null && !condition())
            return false;

        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);

        if (propertyNamesToNotify != null)
            foreach (var name in propertyNamesToNotify)
                OnPropertyChanged(name);

        doAfter?.Invoke();
        return true;
    }

    /// <summary>
    /// Disposes resources used by the object and suppresses finalization.
    /// </summary>
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

/* usage:

public class MainViewModel : StswObservableObject
{
    private string _message;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
}

*/
