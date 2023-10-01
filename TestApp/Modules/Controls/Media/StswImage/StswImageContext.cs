using System.Windows.Media;

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

    /// Stretch
    private Stretch stretch = Stretch.Uniform;
    public Stretch Stretch
    {
        get => stretch;
        set => SetProperty(ref stretch, value);
    }
    #endregion
}
