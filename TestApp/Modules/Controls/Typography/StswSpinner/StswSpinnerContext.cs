using System.Linq;
using System.Windows;

namespace TestApp;
public partial class StswSpinnerContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => Scale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => Scale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Scale)))?.Value ?? default;
        Type = (StswSpinnerType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    [StswObservableProperty] GridLength _scale;
    [StswObservableProperty] StswSpinnerType _type;
}
