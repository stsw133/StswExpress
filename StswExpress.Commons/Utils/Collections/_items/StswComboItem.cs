namespace StswExpress.Commons;
/// <summary>
/// Provides a way to store and display pairs of display and value objects for use in combo boxes.
/// </summary>
[Stsw("0.3.0")]
public partial class StswComboItem : StswObservableObject
{
    /// <summary>
    /// Gets or sets the display text for the item.
    /// </summary>
    [StswObservableProperty] object? _display;

    /// <summary>
    /// Gets or sets the value associated with the item.
    /// </summary>
    [StswObservableProperty] object? _value;
}
