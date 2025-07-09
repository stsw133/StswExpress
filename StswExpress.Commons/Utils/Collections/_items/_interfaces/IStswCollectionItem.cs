using System.ComponentModel;

namespace StswExpress.Commons;
/// <summary>
/// Provides properties for tracking the state and error message of collection items.
/// </summary>
[Stsw("0.3.0")]
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
