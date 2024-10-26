using System.Windows.Media;

namespace TestApp;

public class StswOutlinedTextContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        //Fill = (Brush?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Fill)))?.Value ?? new SolidColorBrush(Colors.Transparent);
        //FontSize = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(FontSize)))?.Value ?? default;
        //Stroke = (Brush?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Stroke)))?.Value ?? new SolidColorBrush(Colors.Transparent);
        //StrokeThickness = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(StrokeThickness)))?.Value ?? default;
    }

    /// Fill
    public Color Fill
    {
        get => _fill;
        set => SetProperty(ref _fill, value);
    }
    private Color _fill = ((SolidColorBrush)App.Current.Resources["StswWindow.Static.Background"]).Color;

    /// FontSize
    public double FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }
    private double _fontSize = 30;

    /// Stroke
    public Color Stroke
    {
        get => _stroke;
        set => SetProperty(ref _stroke, value);
    }
    private Color _stroke = ((SolidColorBrush)App.Current.Resources["StswWindow.Active.Foreground"]).Color;

    /// StrokeThickness
    public double StrokeThickness
    {
        get => _strokeThickness;
        set => SetProperty(ref _strokeThickness, value);
    }
    private double _strokeThickness = 2;
}
