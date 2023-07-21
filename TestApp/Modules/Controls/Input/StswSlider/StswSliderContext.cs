namespace TestApp;

public class StswSliderContext : ControlsContext
{
    #region Properties
    /// Maximum
    private double? maximum = 100;
    public double? Maximum
    {
        get => maximum;
        set => SetProperty(ref maximum, value);
    }
    /// Minimum
    private double? minimum = 0;
    public double? Minimum
    {
        get => minimum;
        set => SetProperty(ref minimum, value);
    }

    /// SelectedValue
    private double? selectedValue = 0;
    public double? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }

    /// SelectionEnd
    private double? selectionEnd = 40;
    public double? SelectionEnd
    {
        get => selectionEnd;
        set => SetProperty(ref selectionEnd, value);
    }
    /// SelectionStart
    private double? selectionStart = 20;
    public double? SelectionStart
    {
        get => selectionStart;
        set => SetProperty(ref selectionStart, value);
    }

    /// TickFrequency
    private double tickFrequency = 5;
    public double TickFrequency
    {
        get => tickFrequency;
        set => SetProperty(ref tickFrequency, value);
    }
    #endregion
}
