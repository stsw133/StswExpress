using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class StswComponentButtonContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswComponentButtonContext()
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

    /// ContentVisibility
    private Visibility contentVisibility = Visibility.Collapsed;
    public Visibility ContentVisibility
    {
        get => contentVisibility;
        set => SetProperty(ref contentVisibility, value);
    }

    /// IconScale
    private double iconScale = 1.33;
    public double IconScale
    {
        get => iconScale;
        set => SetProperty(ref iconScale, value);
    }

    /// IsBusy
    private bool isBusy = false;
    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }
    #endregion
}
