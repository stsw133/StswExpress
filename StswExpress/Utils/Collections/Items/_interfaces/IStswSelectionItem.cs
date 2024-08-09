namespace StswExpress;
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
