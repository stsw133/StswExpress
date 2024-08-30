using System;
using System.Linq;

namespace TestApp;

public class StswTimerControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        EndTime = (TimeSpan?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(EndTime)))?.Value ?? default;
        Format = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Format)))?.Value ?? default;
        IsCountingDown = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsCountingDown)))?.Value ?? default;
        StartStopReset = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(StartStopReset)))?.Value ?? default;
        StartTime = (TimeSpan?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(StartTime)))?.Value ?? default;
    }

    /// EndTime
    public TimeSpan EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }
    private TimeSpan _endTime;

    /// Format
    public string? Format
    {
        get => _format;
        set => SetProperty(ref _format, value);
    }
    private string? _format;

    /// IsCountingDown
    public bool IsCountingDown
    {
        get => _isCountingDown;
        set => SetProperty(ref _isCountingDown, value);
    }
    private bool _isCountingDown;
    
    /// StartStopReset
    public bool? StartStopReset
    {
        get => _startStopReset;
        set => SetProperty(ref _startStopReset, value);
    }
    private bool? _startStopReset;

    /// StartTime
    public TimeSpan StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }
    private TimeSpan _startTime;
}
