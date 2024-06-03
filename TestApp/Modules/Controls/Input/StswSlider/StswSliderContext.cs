namespace TestApp;

public class StswSliderContext : ControlsContext
{
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

    /// SelectedValue
    public double? SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }
    private double? _selectedValue = 0;

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
