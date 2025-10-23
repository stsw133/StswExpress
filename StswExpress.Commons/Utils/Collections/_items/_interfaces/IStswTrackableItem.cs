using System.ComponentModel;

namespace StswExpress.Commons;
/// <summary>
/// Defines a contract for items that can be tracked for changes in state and property notifications.
/// </summary>
public interface IStswTrackableItem : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets the state of the trackable item.
    /// </summary>
    public StswItemState ItemState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show details for the item.
    /// </summary>
    public bool? ShowDetails { get; set; }
}
