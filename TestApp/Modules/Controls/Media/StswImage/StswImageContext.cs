using System.Linq;
using System.Windows.Media;

namespace TestApp;

public class StswImageContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        MenuMode = (StswMenuMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(MenuMode)))?.Value ?? default;
        Stretch = (Stretch?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Stretch)))?.Value ?? default;
    }

    /// MenuMode
    private StswMenuMode menuMode;
    public StswMenuMode MenuMode
    {
        get => menuMode;
        set => SetProperty(ref menuMode, value);
    }

    /// Stretch
    private Stretch stretch;
    public Stretch Stretch
    {
        get => stretch;
        set => SetProperty(ref stretch, value);
    }
}
