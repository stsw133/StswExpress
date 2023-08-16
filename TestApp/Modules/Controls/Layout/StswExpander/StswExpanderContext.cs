using System.Windows;
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

    /// HorizontalAlignment
    private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Stretch;
    public HorizontalAlignment HorizontalAlignment
    {
        get => horizontalAlignment;
        set => SetProperty(ref horizontalAlignment, value);
    }

    /// VerticalAlignment
    private VerticalAlignment verticalAlignment = VerticalAlignment.Top;
    public VerticalAlignment VerticalAlignment
    {
        get => verticalAlignment;
        set => SetProperty(ref verticalAlignment, value);
    }
    #endregion
}
