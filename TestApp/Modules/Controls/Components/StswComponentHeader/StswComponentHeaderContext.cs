using System.Windows;

namespace TestApp;

public class StswComponentHeaderContext : ControlsContext
{
    #region Properties
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
