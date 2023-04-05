using System.Drawing;

namespace TestApp;

public class StswColorPickerContext : StswObservableObject
{
    private Color color = Color.Red;
    public Color Color
    {
        get => color;
        set => SetProperty(ref color, value);
    }
}
