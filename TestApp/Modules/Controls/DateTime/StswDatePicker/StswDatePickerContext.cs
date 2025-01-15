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
        IncrementType = (StswDateTimeIncrementType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IncrementType)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        SelectionUnit = (StswCalendarUnit?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionUnit)))?.Value ?? default;
    }

    /// Format
    public string? Format
    {
        get => _format;
        set => SetProperty(ref _format, value);
    }
    private string? _format;

    /// Icon
    public bool Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }
    private bool _icon;

    /// IncrementType
    public StswDateTimeIncrementType IncrementType
    {
        get => _incrementType;
        set => SetProperty(ref _incrementType, value);
    }
    private StswDateTimeIncrementType _incrementType;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// Maximum
    public DateTime? Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }
    private DateTime? _maximum;

    /// Minimum
    public DateTime? Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }
    private DateTime? _minimum;

    /// SelectedDate
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }
    private DateTime? _selectedDate = DateTime.Now;

    /// SelectionUnit
    public StswCalendarUnit SelectionUnit
    {
        get => _selectionUnit;
        set => SetProperty(ref _selectionUnit, value);
    }
    private StswCalendarUnit _selectionUnit;

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;
}
