using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswPagerContext : ControlsContext
{
    #region Properties
    /// Items
    private List<string> items = Directory.GetFiles($"C:\\KarolStaszewski\\grafika\\inne").ToList();
    public List<string> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

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
