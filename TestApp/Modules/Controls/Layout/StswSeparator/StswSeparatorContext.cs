using System.Windows.Controls;

namespace TestApp;

public class StswSeparatorContext : ControlsContext
{
    #region Properties
    /// Orientation
    private Orientation orientation = Orientation.Horizontal;
    public Orientation Orientation
    {
        get => orientation;
        set => SetProperty(ref orientation, value);
    }
    #endregion
}
