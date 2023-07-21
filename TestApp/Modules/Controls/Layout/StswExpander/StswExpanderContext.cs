using System.Windows.Controls;

namespace TestApp;

public class StswExpanderContext : ControlsContext
{
    #region Properties
    /// ExpandDirection
    private ExpandDirection expandDirection = ExpandDirection.Down;
    public ExpandDirection ExpandDirection
    {
        get => expandDirection;
        set => SetProperty(ref expandDirection, value);
    }
    #endregion
}
