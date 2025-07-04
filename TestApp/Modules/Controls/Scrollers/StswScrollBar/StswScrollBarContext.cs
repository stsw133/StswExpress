using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswScrollBarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsDynamic = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsDynamic)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    [StswObservableProperty] bool _isDynamic;
    [StswObservableProperty] Orientation _orientation;
}
