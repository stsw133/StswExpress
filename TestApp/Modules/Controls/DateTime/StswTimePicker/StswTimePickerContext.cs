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
    private string? format;
    public string? Format
    {
        get => format;
        set => SetProperty(ref format, value);
    }

    /// IncrementType
    private StswTimeSpanIncrementType incrementType;
    public StswTimeSpanIncrementType IncrementType
    {
        get => incrementType;
        set => SetProperty(ref incrementType, value);
    }

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Maximum
    private TimeSpan? maximum;
    public TimeSpan? Maximum
    {
        get => maximum;
        set => SetProperty(ref maximum, value);
    }
    /// Minimum
    private TimeSpan? minimum;
    public TimeSpan? Minimum
    {
        get => minimum;
        set => SetProperty(ref minimum, value);
    }

    /// SelectedTime
    private TimeSpan? selectedTime = new();
    public TimeSpan? SelectedTime
    {
        get => selectedTime;
        set => SetProperty(ref selectedTime, value);
    }

    /// SubControls
    private bool subControls = false;
    public bool SubControls
    {
        get => subControls;
        set => SetProperty(ref subControls, value);
    }
}
