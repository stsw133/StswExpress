using System.Linq;
using System.Windows;

namespace TestApp;

public class StswSpinnerContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => Scale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => Scale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Scale)))?.Value ?? default;
        Type = (StswSpinnerType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    /// Scale
    public GridLength Scale
    {
        get => _scale;
        set => SetProperty(ref _scale, value);
    }
    private GridLength _scale;

    /// Type
    public StswSpinnerType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    private StswSpinnerType _type;
}
