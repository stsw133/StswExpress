using System;
using System.Linq;

namespace TestApp;
public partial class StswDatePickerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Format = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Format)))?.Value ?? default;
        IncrementType = (StswDateTimeIncrementType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IncrementType)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        SelectionUnit = (StswCalendarUnit?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionUnit)))?.Value ?? default;
    }

    [StswCommand] void Clear() => SelectedDate = default;
    [StswCommand] void Randomize() => SelectedDate = new DateTime().AddDays(new Random().Next((DateTime.MaxValue - DateTime.MinValue).Days));

    [StswObservableProperty] string? _format;
    [StswObservableProperty] bool _icon;
    [StswObservableProperty] StswDateTimeIncrementType _incrementType;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] DateTime? _maximum;
    [StswObservableProperty] DateTime? _minimum;
    [StswObservableProperty] DateTime? _selectedDate = DateTime.Now;
    [StswObservableProperty] StswCalendarUnit _selectionUnit;
    [StswObservableProperty] bool _subControls = false;
}
