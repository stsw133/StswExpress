using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswDirectionViewerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        HorizontalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalScrollBarVisibility)))?.Value ?? default;
        VerticalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalScrollBarVisibility)))?.Value ?? default;
    }

    /// HorizontalScrollBarVisibility
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => _horizontalScrollBarVisibility;
        set => SetProperty(ref _horizontalScrollBarVisibility, value);
    }
    private ScrollBarVisibility _horizontalScrollBarVisibility;

    /// VerticalScrollBarVisibility
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => _verticalScrollBarVisibility;
        set => SetProperty(ref _verticalScrollBarVisibility, value);
    }
    private ScrollBarVisibility _verticalScrollBarVisibility;
}
