namespace StswExpress.Commons;
/// <summary>
/// Defines an interface for items that can be selected.
/// </summary>
[StswInfo("0.3.0")]
public interface IStswSelectionItem
{
    /// <summary>
    /// Gets or sets the selection associated with the item.
    /// </summary>
    public bool IsSelected { get; set; }
}
