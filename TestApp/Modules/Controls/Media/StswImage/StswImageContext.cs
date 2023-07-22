namespace TestApp;

public class StswImageContext : ControlsContext
{
    #region Properties
    /// MenuMode
    private StswMenuMode menuMode = StswMenuMode.Full;
    public StswMenuMode MenuMode
    {
        get => menuMode;
        set => SetProperty(ref menuMode, value);
    }
    #endregion
}
