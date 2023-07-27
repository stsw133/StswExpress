using System.Windows.Input;

namespace TestApp;

public class StswDropButtonContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswDropButtonContext()
    {
        OnClickCommand = new StswCommand<string?>(OnClick);
    }

    #region Events
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
