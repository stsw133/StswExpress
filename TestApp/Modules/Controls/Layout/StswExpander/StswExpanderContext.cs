using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;
public partial class StswExpanderContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ExpandDirection = (ExpandDirection?)ThisControlSetters.FirstOrDefault(x => x.Property == StswExpander.ExpandDirectionProperty)?.Value ?? default;
        DropArrowVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property == StswDropArrow.VisibilityProperty)?.Value ?? default;
    }

    [StswObservableProperty] ExpandDirection _expandDirection;
    [StswObservableProperty] Visibility _dropArrowVisibility = Visibility.Visible;
}
