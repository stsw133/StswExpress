using System;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
/// <param name="oldValue"></param>
/// <param name="newValue"></param>
public class StswValueChangedEventArgs<T>(T? oldValue, T? newValue) : EventArgs
{
    public T? OldValue { get; } = oldValue;
    public T? NewValue { get; } = newValue;
}
