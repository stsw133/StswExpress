using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswDirectionViewContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    [StswObservableProperty] Orientation _orientation;
}
