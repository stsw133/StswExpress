using System;
using System.Linq;

namespace TestApp;
public partial class StswDecimalBoxContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Increment = (decimal?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Increment)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswCommand] void Clear() => SelectedValue = default;
    [StswCommand] void Randomize() => SelectedValue = new Random().Next(int.MinValue, int.MaxValue);

    [StswObservableProperty] string? _format = "N2";
    [StswObservableProperty] bool _icon;
    [StswObservableProperty] decimal _increment;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] decimal? _maximum;
    [StswObservableProperty] decimal? _minimum;
    [StswObservableProperty] decimal? _selectedValue = 0;
    [StswObservableProperty] bool _subControls = false;
}
