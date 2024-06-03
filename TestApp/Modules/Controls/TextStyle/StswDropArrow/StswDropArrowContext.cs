using System.Linq;
using System.Windows;

namespace TestApp;

public class StswDropArrowContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => Scale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => Scale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsExpanded = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsExpanded)))?.Value ?? default;
        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Scale)))?.Value ?? default;
    }

    /// IsExpanded
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
    private bool _isExpanded;

    /// Scale
    public GridLength Scale
    {
        get => _scale;
        set => SetProperty(ref _scale, value);
    }
    private GridLength _scale;
}
