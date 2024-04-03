namespace StswExpress;

/// <summary>
/// Defines a contract for scrollable controls.
/// </summary>
public interface IStswScrollableControl
{
    /// <summary>
    /// Gets a <see cref="StswScrollViewer"/> of the control.
    /// </summary>
    public StswScrollViewer GetScrollViewer();
}
