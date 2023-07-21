namespace TestApp;

public class StswIconContext : ControlsContext
{
    #region Properties
    /// Scale
    private double scale = 2;
    public double Scale
    {
        get => scale;
        set => SetProperty(ref scale, value);
    }
    #endregion
}
