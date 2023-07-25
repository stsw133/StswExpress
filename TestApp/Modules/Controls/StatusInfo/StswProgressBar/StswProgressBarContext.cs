namespace TestApp;

public class StswProgressBarContext : ControlsContext
{
    #region Properties
    /// IsIndeterminate
    private bool isIndeterminate = false;
    public bool IsIndeterminate
    {
        get => isIndeterminate;
        set => SetProperty(ref isIndeterminate, value);
    }

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

    /// State
    private StswProgressState state = StswProgressState.Ready;
    public StswProgressState State
    {
        get => state;
        set => SetProperty(ref state, value);
    }

    /// TextMode
    private StswProgressTextMode textMode = StswProgressTextMode.Percentage;
    public StswProgressTextMode TextMode
    {
        get => textMode;
        set => SetProperty(ref textMode, value);
    }
    #endregion
}
