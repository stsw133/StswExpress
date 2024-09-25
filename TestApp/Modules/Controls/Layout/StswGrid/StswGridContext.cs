using System.Linq;

namespace TestApp;

public class StswGridContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        AutoLayoutMode = (StswAutoLayoutMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AutoLayoutMode)))?.Value ?? default;
        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
        VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
    }

    /// AutoLayoutMode
    public StswAutoLayoutMode AutoLayoutMode
    {
        get => _autoLayoutMode;
        set => SetProperty(ref _autoLayoutMode, value);
    }
    private StswAutoLayoutMode _autoLayoutMode;
}
