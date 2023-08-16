using System.Windows.Controls;

namespace TestApp;

public class StswScrollViewerContext : ControlsContext
{
    #region Properties
    /// HorizontalScrollBarVisibility
    private ScrollBarVisibility horizontalScrollBarVisibility = ScrollBarVisibility.Auto;
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => horizontalScrollBarVisibility;
        set => SetProperty(ref horizontalScrollBarVisibility, value);
    }

    /// VerticalScrollBarVisibility
    private ScrollBarVisibility verticalScrollBarVisibility = ScrollBarVisibility.Auto;
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => verticalScrollBarVisibility;
        set => SetProperty(ref verticalScrollBarVisibility, value);
    }
    #endregion
}
