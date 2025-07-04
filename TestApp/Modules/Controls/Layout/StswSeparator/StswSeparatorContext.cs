using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswSeparatorContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        BorderThickness = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(BorderThickness)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    [StswObservableProperty] double _borderThickness;
    [StswObservableProperty] Orientation _orientation;
}
