using System.Linq;

namespace TestApp;

public class StswZoomControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ConstrainToParentBounds = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ConstrainToParentBounds)))?.Value ?? default;
        //MaxScale = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(MaxScale)))?.Value ?? default;
        //MinScale = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(MinScale)))?.Value ?? default;
        ZoomStep = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ZoomStep)))?.Value ?? default;
    }

    /// ConstrainToParentBounds
    public bool ConstrainToParentBounds
    {
        get => _constrainToParentBounds;
        set => SetProperty(ref _constrainToParentBounds, value);
    }
    private bool _constrainToParentBounds;
    
    /// MaxScale
    public double? MaxScale
    {
        get => _maxScale;
        set => SetProperty(ref _maxScale, value);
    }
    private double? _maxScale;

    /// MinScale
    public double? MinScale
    {
        get => _minScale;
        set => SetProperty(ref _minScale, value);
    }
    private double? _minScale;

    /// ZoomStep
    public double ZoomStep
    {
        get => _zoomStep;
        set => SetProperty(ref _zoomStep, value);
    }
    private double _zoomStep;
}
