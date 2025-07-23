using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswExpanderContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ExpandDirection = (ExpandDirection?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ExpandDirection)))?.Value ?? default;
    }

    [StswObservableProperty] ExpandDirection _expandDirection;
}
