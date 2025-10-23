namespace StswExpress.Commons;

/// <summary>
/// Provides a way to store and display pairs of display and value objects for use in selection boxes.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.ChangeName | StswPlannedChanges.LogicChanges, "Will be renamed to StswSelectableItem and will include StswComboItem functionality in future versions.")]
public partial class StswSelectionItem : StswComboItem, IStswSelectionItem
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
