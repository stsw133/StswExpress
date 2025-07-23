using System.Linq;
using System.Windows.Media;

namespace TestApp;
public partial class StswColorPickerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsAlphaEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsAlphaEnabled)))?.Value ?? default;
    }

    [StswObservableProperty] bool _isAlphaEnabled;
    [StswObservableProperty] Color _selectedColor = Color.FromRgb(24, 240, 24);
}
