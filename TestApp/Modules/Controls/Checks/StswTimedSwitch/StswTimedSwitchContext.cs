using System;
using System.Linq;

namespace TestApp;

public class StswTimedSwitchContext : ControlsContext
{
    public StswCommand EnableTimerCommand => new(() => IsChecked = true);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsChecked = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsChecked)))?.Value ?? default;
        SwitchTime = (TimeSpan?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SwitchTime)))?.Value ?? default;
    }

    /// IsChecked
    private bool isChecked;
    public bool IsChecked
    {
        get => isChecked;
        set => SetProperty(ref isChecked, value);
    }

    /// SwitchTime
    private TimeSpan switchTime;
    public TimeSpan SwitchTime
    {
        get => switchTime;
        set => SetProperty(ref switchTime, value);
    }
}
