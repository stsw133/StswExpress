using System;
using System.Linq;

namespace TestApp;

public class StswCalendarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        SelectionMode = (StswCalendarMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionMode)))?.Value ?? default;
    }

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

    /// SelectionMode
    public StswCalendarMode SelectionMode
    {
        get => _selectionMode;
        set => SetProperty(ref _selectionMode, value);
    }
    private StswCalendarMode _selectionMode;
}
