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
        get => display;
        set => SetProperty(ref display, value);
    }
    private object? display;

    /// <summary>
    /// Gets or sets the value associated with the item.
    /// </summary>
    public object? Value
    {
        get => value;
        set => SetProperty(ref this.value, value);
    }
    private object? value;
}

/// <summary>
/// Defines an interface for items that can be selected.
/// </summary>
public interface IStswSelection
{
    /// <summary>
    /// Gets or sets the selection associated with the item.
    /// </summary>
    public bool IsSelected { get; set; }
}

/// <summary>
/// Provides a way to store and display pairs of display and value objects for use in selection boxes.
/// </summary>
public class StswSelectionItem : StswComboItem, IStswSelection
{
    /// <summary>
    /// Gets or sets the selection associated with the item.
    /// </summary>
    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }
    private bool isSelected;
}
