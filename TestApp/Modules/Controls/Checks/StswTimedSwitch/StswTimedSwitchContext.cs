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
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
    private bool _isChecked;

    /// SwitchTime
    public TimeSpan SwitchTime
    {
        get => _switchTime;
        set => SetProperty(ref _switchTime, value);
    }
    private TimeSpan _switchTime;
}
