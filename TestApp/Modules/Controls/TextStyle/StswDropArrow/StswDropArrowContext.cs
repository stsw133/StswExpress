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
    private bool isExpanded;
    public bool IsExpanded
    {
        get => isExpanded;
        set => SetProperty(ref isExpanded, value);
    }

    /// Scale
    private GridLength scale;
    public GridLength Scale
    {
        get => scale;
        set => SetProperty(ref scale, value);
    }
}
