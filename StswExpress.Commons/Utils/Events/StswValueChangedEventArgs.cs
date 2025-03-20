namespace StswExpress.Commons;

/// <summary>
/// Provides data for an event that signals a change in a value.
/// </summary>
/// <typeparam name="T">The type of the value that changed.</typeparam>
/// <param name="oldValue">The previous value before the change.</param>
/// <param name="newValue">The new value after the change.</param>
public class StswValueChangedEventArgs<T>(T? oldValue, T? newValue) : EventArgs
{
    /// <summary>
    /// Gets the previous value before the change.
    /// </summary>
    public T? OldValue { get; } = oldValue;

    /// <summary>
    /// Gets the new value after the change.
    /// </summary>
    public T? NewValue { get; } = newValue;
}
