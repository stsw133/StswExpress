using System.Windows.Media;

namespace TestApp;

public class StswColorSelectorContext : ControlsContext
{
    /// SelectedColor
    public Color SelectedColor
    {
        get => _selectedColor;
        set => SetProperty(ref _selectedColor, value);
    }
    private Color _selectedColor = Color.FromRgb(24, 240, 24);
}
