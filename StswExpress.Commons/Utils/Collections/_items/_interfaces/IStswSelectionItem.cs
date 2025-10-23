namespace StswExpress.Commons;
/// <summary>
/// Defines an interface for items that can be selected.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.ChangeName, "Will be renamed to IStswSelectableItem in future versions.")]
public interface IStswSelectionItem
{
    /// <summary>
    /// Gets or sets the selection associated with the item.
    /// </summary>
    public bool IsSelected { get; set; }
}
