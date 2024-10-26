using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswScrollBarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsDynamic = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsDynamic)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    /// IsDynamic
    public bool IsDynamic
    {
        get => _isDynamic;
        set => SetProperty(ref _isDynamic, value);
    }
    private bool _isDynamic;

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;
}
