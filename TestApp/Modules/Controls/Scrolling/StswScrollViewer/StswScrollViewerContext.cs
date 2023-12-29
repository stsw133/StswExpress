using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswScrollViewerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        HorizontalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalScrollBarVisibility)))?.Value ?? default;
        VerticalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalScrollBarVisibility)))?.Value ?? default;
        isDynamic = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsDynamic)))?.Value ?? default;
    }

    /// HorizontalScrollBarVisibility
    private ScrollBarVisibility horizontalScrollBarVisibility;
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => horizontalScrollBarVisibility;
        set => SetProperty(ref horizontalScrollBarVisibility, value);
    }

    /// VerticalScrollBarVisibility
    private ScrollBarVisibility verticalScrollBarVisibility;
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => verticalScrollBarVisibility;
        set => SetProperty(ref verticalScrollBarVisibility, value);
    }

    /// IsDynamic
    private bool isDynamic;
    public bool IsDynamic
    {
        get => isDynamic;
        set => SetProperty(ref isDynamic, value);
    }
}
