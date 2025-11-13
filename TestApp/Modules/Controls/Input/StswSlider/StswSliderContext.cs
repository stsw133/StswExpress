using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswSliderContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        IsMoveToPointEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSlider.IsMoveToPointEnabledProperty)?.Value ?? default;
        //Maximum = (double?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSlider.MaximumProperty)?.Value ?? default;
        //Minimum = (double?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSlider.MinimumProperty)?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSlider.OrientationProperty)?.Value ?? default;
        SelectedValue = (double?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSlider.ValueProperty)?.Value ?? default;
        //SelectionEnd = (double?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSlider.SelectionEndProperty)?.Value ?? default;
        //SelectionStart = (double?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSlider.SelectionStartProperty)?.Value ?? default;
        SliderMode = (StswSliderMode?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSlider.SliderModeProperty)?.Value ?? default;
        //TickFrequency = (double?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSlider.TickFrequencyProperty)?.Value ?? default;
    }

    [StswObservableProperty] bool _isMoveToPointEnabled;
    [StswObservableProperty] double? _maximum = 100;
    [StswObservableProperty] double? _minimum = 0;
    [StswObservableProperty] Orientation _orientation;
    [StswObservableProperty] double? _selectedValue;
    [StswObservableProperty] double? _selectionEnd = 40;
    [StswObservableProperty] double? _selectionStart = 20;
    [StswObservableProperty] StswSliderMode _sliderMode;
    [StswObservableProperty] double _tickFrequency = 5;
}
