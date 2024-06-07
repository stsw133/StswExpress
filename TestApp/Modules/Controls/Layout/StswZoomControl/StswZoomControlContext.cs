using System.Linq;

namespace TestApp;

public class StswZoomControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        MinScale = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(MinScale)))?.Value ?? default;
    }

    /// MinScale
    public double MinScale
    {
        get => _minScale;
        set => SetProperty(ref _minScale, value);
    }
    private double _minScale;
}
