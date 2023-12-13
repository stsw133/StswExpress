using System.ComponentModel;
using System.Data;

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
public interface IStswSelectionItem
{
    /// <summary>
    /// Gets or sets the selection associated with the item.
    /// </summary>
    public bool IsSelected { get; set; }
}

/// <summary>
/// Provides a way to store and display pairs of display and value objects for use in selection boxes.
/// </summary>
public class StswSelectionItem : StswComboItem, IStswSelectionItem
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

/// <summary>
/// Provides properties for tracking the state and error message of collection items.
/// </summary>
public interface IStswCollectionItem : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets the error message associated with the collection item.
    /// </summary>
    public string? ItemMessage { get; set; }

    /// <summary>
    /// Gets or sets the state of the collection item.
    /// </summary>
    public StswItemState ItemState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show details for the collection item.
    /// </summary>
    public bool? ShowDetails { get; set; }
}
