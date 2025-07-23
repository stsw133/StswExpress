using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswScrollBarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        DynamicMode = (StswScrollDynamicMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(DynamicMode)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    [StswObservableProperty] StswScrollDynamicMode _dynamicMode;
    [StswObservableProperty] Orientation _orientation;
}
