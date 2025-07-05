namespace StswExpress.Commons;
/// <summary>
/// Defines an interface for items that can be selected.
/// </summary>
[Stsw("0.3.0", Changes = StswPlannedChanges.None)]
public interface IStswSelectionItem
{
    /// <summary>
    /// Gets or sets the selection associated with the item.
    /// </summary>
    public bool IsSelected { get; set; }
}
