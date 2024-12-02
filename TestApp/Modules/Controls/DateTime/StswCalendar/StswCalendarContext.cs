using System;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswCalendarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        SelectionMode = (SelectionMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionMode)))?.Value ?? default;
        SelectionUnit = (StswCalendarUnit?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionUnit)))?.Value ?? default;
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
    public SelectionMode SelectionMode
    {
        get => _selectionMode;
        set => SetProperty(ref _selectionMode, value);
    }
    private SelectionMode _selectionMode;

    /// SelectionUnit
    public StswCalendarUnit SelectionUnit
    {
        get => _selectionUnit;
        set => SetProperty(ref _selectionUnit, value);
    }
    private StswCalendarUnit _selectionUnit;
}
