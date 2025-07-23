using System.Windows.Media;

namespace TestApp;
public partial class StswOutlinedTextContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        //Fill = (Brush?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Fill)))?.Value ?? new SolidColorBrush(Colors.Transparent);
        //FontSize = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(FontSize)))?.Value ?? default;
        //Stroke = (Brush?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Stroke)))?.Value ?? new SolidColorBrush(Colors.Transparent);
        //StrokeThickness = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(StrokeThickness)))?.Value ?? default;
    }

    [StswObservableProperty] Color _fill = ((SolidColorBrush)App.Current.Resources["StswBox.Static.Background"]).Color;
    [StswObservableProperty] double _fontSize = 30;
    [StswObservableProperty] Color _stroke = ((SolidColorBrush)App.Current.Resources["StswWindow.Active.Foreground"]).Color;
    [StswObservableProperty] double _strokeThickness = 2;
}
