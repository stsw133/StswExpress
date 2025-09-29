using System;
using System.Linq;

namespace TestApp;
public partial class StswTimePickerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        AreButtonsVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AreButtonsVisible)))?.Value ?? default;
        Format = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Format)))?.Value ?? default;
        IncrementType = (StswTimeSpanIncrementType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IncrementType)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswCommand] void Clear() => SelectedTime = null;
    [StswCommand] void Randomize() => SelectedTime = new TimeSpan(0, 0, 0, new Random().Next(int.MaxValue));

    [StswObservableProperty] bool _areButtonsVisible;
    [StswObservableProperty] string? _format;
    [StswObservableProperty] bool _icon;
    [StswObservableProperty] StswTimeSpanIncrementType _incrementType;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] TimeSpan? _maximum;
    [StswObservableProperty] TimeSpan? _minimum;
    [StswObservableProperty] TimeSpan? _selectedTime = new();
    [StswObservableProperty] bool _subControls = false;
}
