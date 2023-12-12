using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class StswComponentDropContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswComponentDropContext()
    {
        OnClickCommand = new StswCommand<string?>(OnClick);
    }

    #region Events and methods
    /// Command: on click
    private void OnClick(string? parameter)
    {
        if (int.TryParse(parameter, out var result))
            ClickOption = result;
        IsDropDownOpen = false;
    }
    #endregion

    #region Properties
    /// ClickOption
    private int clickOption;
    public int ClickOption
    {
        get => clickOption;
        set => SetProperty(ref clickOption, value);
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

    /// IsDropDownOpen
    private bool isDropDownOpen = false;
    public bool IsDropDownOpen
    {
        get => isDropDownOpen;
        set => SetProperty(ref isDropDownOpen, value);
    }

    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }
    #endregion
}
