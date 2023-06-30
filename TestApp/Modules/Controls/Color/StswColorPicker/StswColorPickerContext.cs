using System.Windows.Media;

namespace TestApp;

public class StswColorPickerContext : StswObservableObject
{
    private Color color = Color.FromRgb(24, 240, 24);
    public Color Color
    {
        get => color;
        set => SetProperty(ref color, value);
    }
}
