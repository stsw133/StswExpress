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

    #region Properties
    /// IsAlphaEnabled
    private bool isAlphaEnabled;
    public bool IsAlphaEnabled
    {
        get => isAlphaEnabled;
        set => SetProperty(ref isAlphaEnabled, value);
    }

    /// SelectedColor
    private Color selectedColor = Color.FromRgb(24, 240, 24);
    public Color SelectedColor
    {
        get => selectedColor;
        set => SetProperty(ref selectedColor, value);
    }
    #endregion
}
