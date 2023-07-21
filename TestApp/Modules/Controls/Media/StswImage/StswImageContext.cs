namespace TestApp;

public class StswImageContext : ControlsContext
{
    #region Properties
    /// MenuMode
    private StswImage.MenuModes menuMode = StswImage.MenuModes.Full;
    public StswImage.MenuModes MenuMode
    {
        get => menuMode;
        set => SetProperty(ref menuMode, value);
    }
    #endregion
}
