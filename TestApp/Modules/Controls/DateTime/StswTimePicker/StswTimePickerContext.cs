using System;
using System.Linq;

namespace TestApp;

public class StswTimePickerContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedTime = default);
    public StswCommand RandomizeCommand => new(() => SelectedTime = new TimeSpan(0, 0, 0, new Random().Next(int.MaxValue)));

    public override void SetDefaults()
    {
        base.SetDefaults();

        Format = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Format)))?.Value ?? default;
        IncrementType = (StswTimeSpanIncrementType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IncrementType)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// Format
    public string? Format
    {
        get => _format;
        set => SetProperty(ref _format, value);
    }
    private string? _format;

    /// IncrementType
    public StswTimeSpanIncrementType IncrementType
    {
        get => _incrementType;
        set => SetProperty(ref _incrementType, value);
    }
    private StswTimeSpanIncrementType _incrementType;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// Maximum
    public TimeSpan? Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }
    private TimeSpan? _maximum;

    /// Minimum
    public TimeSpan? Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }
    private TimeSpan? _minimum;

    /// SelectedTime
    public TimeSpan? SelectedTime
    {
        get => _selectedTime;
        set => SetProperty(ref _selectedTime, value);
    }
    private TimeSpan? _selectedTime = new();

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;
}
