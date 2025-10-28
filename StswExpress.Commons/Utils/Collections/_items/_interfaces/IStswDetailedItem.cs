using System.ComponentModel;

namespace StswExpress.Commons;
/// <summary>
/// Defines a contract for items that expose a togglable details view state.
/// </summary>
public interface IStswDetailedItem : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets a value indicating whether to show details for the item.
    /// </summary>
    public bool? ShowDetails { get; set; }
}
