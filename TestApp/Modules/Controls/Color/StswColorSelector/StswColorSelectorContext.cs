using System.Windows.Media;

namespace TestApp;

public class StswColorSelectorContext : ControlsContext
{
    /// SelectedColor
    private Color selectedColor = Color.FromRgb(24, 240, 24);
    public Color SelectedColor
    {
        get => selectedColor;
        set => SetProperty(ref selectedColor, value);
    }
}
