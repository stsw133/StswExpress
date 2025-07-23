using System.Linq;
using System.Windows;

namespace TestApp;
public partial class StswSubLabelContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
    }

    [StswCommand] void SetGridLengthAuto() => IconScale = GridLength.Auto;
    [StswCommand] void SetGridLengthFill() => IconScale = new GridLength(1, GridUnitType.Star);

    [StswObservableProperty] GridLength _iconScale;
    [StswObservableProperty] bool _isBusy;
    [StswObservableProperty] bool _isContentVisible;
}
