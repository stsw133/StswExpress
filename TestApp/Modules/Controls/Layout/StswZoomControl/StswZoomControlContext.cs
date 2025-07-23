using System.Linq;

namespace TestApp;
public partial class StswZoomControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ConstrainToParentBounds = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ConstrainToParentBounds)))?.Value ?? default;
        //MaxScale = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(MaxScale)))?.Value ?? default;
        //MinScale = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(MinScale)))?.Value ?? default;
        ZoomStep = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ZoomStep)))?.Value ?? default;
    }

    [StswObservableProperty] bool _constrainToParentBounds;
    [StswObservableProperty] double? _maxScale;
    [StswObservableProperty] double? _minScale;
    [StswObservableProperty] double _zoomStep;
}
