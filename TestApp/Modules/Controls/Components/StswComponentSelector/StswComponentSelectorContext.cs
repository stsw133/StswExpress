using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class StswComponentSelectorContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswComponentSelectorContext()
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

    /// HeaderVisibility
    private Visibility headerVisibility = Visibility.Collapsed;
    public Visibility HeaderVisibility
    {
        get => headerVisibility;
        set => SetProperty(ref headerVisibility, value);
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
