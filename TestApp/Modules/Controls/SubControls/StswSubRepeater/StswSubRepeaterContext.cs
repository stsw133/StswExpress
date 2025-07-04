using System.Linq;
using System.Windows;

namespace TestApp;
public partial class StswSubRepeaterContext : ControlsContext
{
    public StswCommand OnClickCommand => new(() => ClickCounter++);
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
    }

    [StswObservableProperty] int _clickCounter;
    [StswObservableProperty] GridLength _iconScale;
    [StswObservableProperty] bool _isBusy;
    [StswObservableProperty] bool _isContentVisible;
}
