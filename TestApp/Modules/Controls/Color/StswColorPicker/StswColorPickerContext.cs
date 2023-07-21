using System.Windows.Media;

namespace TestApp;

public class StswColorPickerContext : ControlsContext
{
    #region Properties
    /// IsAlphaEnabled
    private bool isAlphaEnabled = true;
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
