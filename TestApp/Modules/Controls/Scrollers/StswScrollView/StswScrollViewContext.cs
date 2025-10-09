using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswScrollViewContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        DynamicMode = (StswDynamicVisibilityMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(DynamicMode)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        HorizontalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalScrollBarVisibility)))?.Value ?? default;
        VerticalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalScrollBarVisibility)))?.Value ?? default;
    }

    [StswObservableProperty] StswDynamicVisibilityMode _dynamicMode;
    [StswObservableProperty] bool _isBusy;
    [StswObservableProperty] ScrollBarVisibility _horizontalScrollBarVisibility;
    [StswObservableProperty] ScrollBarVisibility _verticalScrollBarVisibility;
}
