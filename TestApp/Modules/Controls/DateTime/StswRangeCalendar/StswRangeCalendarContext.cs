using System;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswRangeCalendarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        SelectionMode = (SelectionMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionMode)))?.Value ?? default;
        SelectionUnit = (StswCalendarUnit?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionUnit)))?.Value ?? default;
    }

    [StswObservableProperty] DateTime? _maximum;
    [StswObservableProperty] DateTime? _minimum;
    [StswObservableProperty] StswDateRange? _selectedRange = new(DateTime.Now, DateTime.Now);
    [StswObservableProperty] SelectionMode _selectionMode;
    [StswObservableProperty] StswCalendarUnit _selectionUnit;
}
