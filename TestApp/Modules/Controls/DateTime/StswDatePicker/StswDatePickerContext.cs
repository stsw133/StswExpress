using System;
using System.Linq;

namespace TestApp;

public class StswDatePickerContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedDate = default);
    public StswCommand RandomizeCommand => new(() => SelectedDate = new DateTime().AddDays(new Random().Next((DateTime.MaxValue - DateTime.MinValue).Days)));

    public override void SetDefaults()
    {
        base.SetDefaults();

        Format = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Format)))?.Value ?? default;
        IncrementType = (StswDateIncrementType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IncrementType)))?.Value ?? default;
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
    private StswDateIncrementType incrementType;
    public StswDateIncrementType IncrementType
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
    private DateTime? maximum;
    public DateTime? Maximum
    {
        get => maximum;
        set => SetProperty(ref maximum, value);
    }
    /// Minimum
    private DateTime? minimum;
    public DateTime? Minimum
    {
        get => minimum;
        set => SetProperty(ref minimum, value);
    }

    /// SelectedDate
    private DateTime? selectedDate = DateTime.Now;
    public DateTime? SelectedDate
    {
        get => selectedDate;
        set => SetProperty(ref selectedDate, value);
    }

    /// SubControls
    private bool subControls = false;
    public bool SubControls
    {
        get => subControls;
        set => SetProperty(ref subControls, value);
    }
}
