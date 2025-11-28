using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswNavigationContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        TabStripPlacement = (Dock?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TabStripPlacement)))?.Value ?? default;
    }

    [StswObservableProperty] Dock _tabStripPlacement;
}
