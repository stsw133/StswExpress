using System.Linq;

namespace TestApp;
public partial class StswProgressBarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        VerticalAlignment = System.Windows.VerticalAlignment.Top;

        IsIndeterminate = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsIndeterminate)))?.Value ?? default;
        State = (StswProgressState?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(State)))?.Value ?? default;
        TextMode = (StswProgressTextMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TextMode)))?.Value ?? default;
    }

    [StswObservableProperty] bool _isIndeterminate;
    [StswObservableProperty] double? _maximum = 100;
    [StswObservableProperty] double? _minimum = 0;
    [StswObservableProperty] double? _selectedValue = 0;
    [StswObservableProperty] StswProgressState _state;
    [StswObservableProperty] StswProgressTextMode _textMode;
}
