namespace StswExpress.Commons;

/// <summary>
/// Provides a way to store and display pairs of display and value objects for use in selection boxes.
/// </summary>
[StswInfo("0.3.0")]
public partial class StswSelectionItem : StswComboItem, IStswSelectionItem
{
    /// <summary>
    /// Gets or sets the selection associated with the item.
    /// </summary>
    [StswObservableProperty] bool _isSelected;
}
