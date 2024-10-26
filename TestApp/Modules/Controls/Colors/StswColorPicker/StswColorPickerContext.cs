using System.Linq;
using System.Windows.Media;

namespace TestApp;

public class StswColorPickerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsAlphaEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsAlphaEnabled)))?.Value ?? default;
    }

    /// IsAlphaEnabled
    public bool IsAlphaEnabled
    {
        get => _isAlphaEnabled;
        set => SetProperty(ref _isAlphaEnabled, value);
    }
    private bool _isAlphaEnabled;

    /// SelectedColor
    public Color SelectedColor
    {
        get => _selectedColor;
        set => SetProperty(ref _selectedColor, value);
    }
    private Color _selectedColor = Color.FromRgb(24, 240, 24);
}
