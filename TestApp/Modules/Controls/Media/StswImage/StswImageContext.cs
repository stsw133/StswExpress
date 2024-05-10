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
    public StswMenuMode MenuMode
    {
        get => _menuMode;
        set => SetProperty(ref _menuMode, value);
    }
    private StswMenuMode _menuMode;

    /// Stretch
    public Stretch Stretch
    {
        get => _stretch;
        set => SetProperty(ref _stretch, value);
    }
    private Stretch _stretch;
}
