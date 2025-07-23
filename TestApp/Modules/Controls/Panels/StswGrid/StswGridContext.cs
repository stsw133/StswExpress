using System.Linq;

namespace TestApp;
public partial class StswGridContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        AutoLayoutMode = (StswAutoLayoutMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AutoLayoutMode)))?.Value ?? default;
        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
        VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
    }

    [StswObservableProperty] StswAutoLayoutMode _autoLayoutMode;
}
