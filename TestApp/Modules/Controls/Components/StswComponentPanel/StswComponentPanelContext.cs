using System.Windows.Input;

namespace TestApp;

public class StswComponentPanelContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswComponentPanelContext()
    {
        OnClickCommand = new StswCommand(OnClick);
    }

    #region Events and methods
    /// OnClickCommand
    private void OnClick() => ClickCounter++;
    #endregion

    #region Properties
    /// ClickCounter
    private int clickCounter;
    public int ClickCounter
    {
        get => clickCounter;
        set => SetProperty(ref clickCounter, value);
    }

    /// IconScale
    private double iconScale = 1.33;
    public double IconScale
    {
        get => iconScale;
        set => SetProperty(ref iconScale, value);
    }
    #endregion
}
