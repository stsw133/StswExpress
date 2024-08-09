namespace StswExpress;
/// <summary>
/// Provides a way to store and display pairs of display and value objects for use in combo boxes.
/// </summary>
public class StswComboItem : StswObservableObject
{
    /// <summary>
    /// Gets or sets the display text for the item.
    /// </summary>
    public object? Display
    {
        get => _display;
        set => SetProperty(ref _display, value);
    }
    private object? _display;

    /// <summary>
    /// Gets or sets the value associated with the item.
    /// </summary>
    public object? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
    private object? _value;
}
