using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswSliderContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        //Maximum = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Maximum)))?.Value ?? default;
        //Minimum = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Minimum)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
        SelectedValue = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectedValue)))?.Value ?? default;
        //SelectionEnd = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionEnd)))?.Value ?? default;
        //SelectionStart = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionStart)))?.Value ?? default;
        //TickFrequency = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TickFrequency)))?.Value ?? default;
    }

    [StswObservableProperty] double? _maximum = 100;
    [StswObservableProperty] double? _minimum = 0;
    [StswObservableProperty] Orientation _orientation;
    [StswObservableProperty] double? _selectedValue;
    [StswObservableProperty] double? _selectionEnd = 40;
    [StswObservableProperty] double? _selectionStart = 20;
    [StswObservableProperty] double _tickFrequency = 5;
}
