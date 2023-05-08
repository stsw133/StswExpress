using System.Windows.Media;

namespace TestApp;

public class StswColorBoxContext : StswObservableObject
{
    private Color color = Color.FromRgb(255, 0, 0);
    public Color Color
    {
        get => color;
        set => SetProperty(ref color, value);
    }
}
