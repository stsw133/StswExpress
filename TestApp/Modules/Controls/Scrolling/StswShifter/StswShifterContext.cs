using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswShifterContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        HorizontalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalScrollBarVisibility)))?.Value ?? default;
        VerticalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalScrollBarVisibility)))?.Value ?? default;
    }

    #region Properties
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
    #endregion
}
