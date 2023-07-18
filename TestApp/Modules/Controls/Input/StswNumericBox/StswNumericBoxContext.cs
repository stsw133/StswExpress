namespace TestApp;

public class StswNumericBoxContext : ControlsContext
{
    #region Properties
    /// Number
    private double number = 0;
    public double Number
    {
        get => number;
        set => SetProperty(ref number, value);
    }
    #endregion
}
