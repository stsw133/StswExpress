using System.ComponentModel;

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
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    private bool _isSelected;
}

/// <summary>
/// Provides properties for tracking the state and error message of collection items.
/// </summary>
public interface IStswCollectionItem : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets the state of the collection item.
    /// </summary>
    public StswItemState ItemState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show details for the collection item.
    /// </summary>
    public bool? ShowDetails { get; set; }
}
