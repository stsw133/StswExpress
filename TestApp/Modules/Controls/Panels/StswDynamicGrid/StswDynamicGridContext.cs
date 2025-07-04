using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;
public partial class StswDynamicGridContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Columns = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Columns)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
        Rows = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Rows)))?.Value ?? default;
        Spacing = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Spacing)))?.Value ?? default;
        StretchColumnIndex = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(StretchColumnIndex)))?.Value ?? default;
        StretchRowIndex = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(StretchRowIndex)))?.Value ?? default;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
    }

    [StswObservableProperty] int _columns;
    [StswObservableProperty] Orientation? _orientation;
    [StswObservableProperty] int _rows;
    [StswObservableProperty] double _spacing;
    [StswObservableProperty] int _stretchColumnIndex;
    [StswObservableProperty] int _stretchRowIndex;
}
