namespace StswExpress.Commons;

/// <summary>
/// Provides a way to store and display pairs of display and value objects for use in selectors.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.LogicChanges, "Will include StswComboItem functionality in future versions.")]
public partial class StswSelectableItem : StswComboItem, IStswSelectableItem
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
