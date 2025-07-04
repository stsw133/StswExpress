using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswScrollViewContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsDynamic = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsDynamic)))?.Value ?? default;
        HorizontalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalScrollBarVisibility)))?.Value ?? default;
        VerticalScrollBarVisibility = (ScrollBarVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalScrollBarVisibility)))?.Value ?? default;
    }

    [StswObservableProperty] bool _isBusy;
    [StswObservableProperty] bool _isDynamic;
    [StswObservableProperty] ScrollBarVisibility _horizontalScrollBarVisibility;
    [StswObservableProperty] ScrollBarVisibility _verticalScrollBarVisibility;
}
