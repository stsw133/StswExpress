using System.Windows.Input;

namespace TestApp;

public class StswButtonContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswButtonContext()
    {
        OnClickCommand = new StswRelayCommand(OnClick);
    }

    #region Events
    /// Command: on click
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

    /// IsDefault
    private bool isDefault = false;
    public bool IsDefault
    {
        get => isDefault;
        set => SetProperty(ref isDefault, value);
    }

    /// Type
    private int type = 0;
    public int Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }
    #endregion
}
