using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswSliderContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        //Maximum = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Maximum)))?.Value ?? default;
        //Minimum = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Minimum)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
        SelectedValue = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectedValue)))?.Value ?? default;
        //SelectionEnd = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionEnd)))?.Value ?? default;
        //SelectionStart = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionStart)))?.Value ?? default;
        //TickFrequency = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TickFrequency)))?.Value ?? default;
    }

    /// Maximum
    public double? Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }
    private double? _maximum = 100;

    /// Minimum
    public double? Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }
    private double? _minimum = 0;

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;

    /// SelectedValue
    public double? SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }
    private double? _selectedValue;

    /// SelectionEnd
    public double? SelectionEnd
    {
        get => _selectionEnd;
        set => SetProperty(ref _selectionEnd, value);
    }
    private double? _selectionEnd = 40;

    /// SelectionStart
    public double? SelectionStart
    {
        get => _selectionStart;
        set => SetProperty(ref _selectionStart, value);
    }
    private double? _selectionStart = 20;

    /// TickFrequency
    public double TickFrequency
    {
        get => _tickFrequency;
        set => SetProperty(ref _tickFrequency, value);
    }
    private double _tickFrequency = 5;
}
