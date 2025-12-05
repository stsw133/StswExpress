using System;
using System.Linq;

namespace TestApp.Wpf;
public partial class StswTimerControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        EndTime = (TimeSpan?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(EndTime)))?.Value ?? default;
        Format = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Format)))?.Value ?? default;
        IsCountingDown = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsCountingDown)))?.Value ?? default;
        IsRunning = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsRunning)))?.Value ?? default;
        ResetOnStart = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ResetOnStart)))?.Value ?? default;
        StartTime = (TimeSpan?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(StartTime)))?.Value ?? default;
    }
    
    [StswObservableProperty] TimeSpan _endTime;
    [StswObservableProperty] string? _format;
    [StswObservableProperty] bool _isCountingDown;
    [StswObservableProperty] bool _isRunning;
    [StswObservableProperty] bool _resetOnStart;
    [StswObservableProperty] TimeSpan _startTime;
}
