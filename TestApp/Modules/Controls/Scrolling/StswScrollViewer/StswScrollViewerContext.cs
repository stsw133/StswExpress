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
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;
    
    /// IsDynamic
    public bool IsDynamic
    {
        get => _isDynamic;
        set => SetProperty(ref _isDynamic, value);
    }
    private bool _isDynamic;

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
