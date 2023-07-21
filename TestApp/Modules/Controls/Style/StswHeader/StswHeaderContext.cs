using System.Windows;

namespace TestApp;

public class StswHeaderContext : ControlsContext
{
    #region Properties
    /// ContentVisibility
    private Visibility contentVisibility = Visibility.Visible;
    public Visibility ContentVisibility
    {
        get => contentVisibility;
        set => SetProperty(ref contentVisibility, value);
    }

    /// IconScale
    private double iconScale = 1.5;
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

    /// ShowDescription
    private bool showDescription = false;
    public bool ShowDescription
    {
        get => showDescription;
        set => SetProperty(ref showDescription, value);
    }
    #endregion
}
