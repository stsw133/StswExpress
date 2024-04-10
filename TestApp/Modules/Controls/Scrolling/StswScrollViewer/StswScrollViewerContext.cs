using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswScrollViewerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsDynamic = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsDynamic)))?.Value ?? default;
        HorizontalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalScrollBarVisibility)))?.Value ?? default;
        VerticalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalScrollBarVisibility)))?.Value ?? default;
    }

    /// IsBusy
    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }
    
    /// IsDynamic
    private bool isDynamic;
    public bool IsDynamic
    {
        get => isDynamic;
        set => SetProperty(ref isDynamic, value);
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
}
