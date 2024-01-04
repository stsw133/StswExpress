using System;
using System.Linq;

namespace TestApp;

public class StswTimedSwitchContext : ControlsContext
{
    public StswCommand EnableTimerCommand => new(() => IsTimerEnabled = true);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsTimerEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsTimerEnabled)))?.Value ?? default;
        SwitchTime = (TimeSpan?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SwitchTime)))?.Value ?? default;
    }

    /// IsTimerEnabled
    private bool isTimerEnabled;
    public bool IsTimerEnabled
    {
        get => isTimerEnabled;
        set => SetProperty(ref isTimerEnabled, value);
    }

    /// SwitchTime
    private TimeSpan switchTime;
    public TimeSpan SwitchTime
    {
        get => switchTime;
        set => SetProperty(ref switchTime, value);
    }
}
