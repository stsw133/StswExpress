using System.Linq;
using System.Windows;

namespace TestApp;
public partial class StswProgressRingContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => Scale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => Scale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsIndeterminate = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsIndeterminate)))?.Value ?? default;
        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Scale)))?.Value ?? default;
        State = (StswProgressState?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(State)))?.Value ?? default;
        TextMode = (StswProgressTextMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TextMode)))?.Value ?? default;
    }

    [StswObservableProperty] bool _isIndeterminate;
    [StswObservableProperty] double? _maximum = 100;
    [StswObservableProperty] double? _minimum = 0;
    [StswObservableProperty] GridLength _scale;
    [StswObservableProperty] double? _selectedValue = 0;
    [StswObservableProperty] StswProgressState _state;
    [StswObservableProperty] StswProgressTextMode _textMode;
}
