using System;
using System.Linq;

namespace TestApp;
public partial class StswTimedSwitchContext : ControlsContext
{
    public StswCommand EnableTimerCommand => new(() => IsChecked = true);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsChecked = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsChecked)))?.Value ?? default;
        SwitchTime = (TimeSpan?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SwitchTime)))?.Value ?? default;
    }

    [StswObservableProperty] bool _isChecked;
    [StswObservableProperty] TimeSpan _switchTime;
}
