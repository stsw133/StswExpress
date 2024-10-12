using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswSeparatorContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        BorderThickness = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(BorderThickness)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    /// BorderThickness
    public double BorderThickness
    {
        get => _borderThickness;
        set => SetProperty(ref _borderThickness, value);
    }
    private double _borderThickness;

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;
}
