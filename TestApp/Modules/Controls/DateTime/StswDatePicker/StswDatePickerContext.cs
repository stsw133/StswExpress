using System;
using System.Linq;

namespace TestApp;
public partial class StswDatePickerContext : ControlsContext
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
