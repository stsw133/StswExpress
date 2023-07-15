using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TestApp;

public class StswCheckBoxContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswCheckBoxContext()
    {
        OnClickCommand = new StswRelayCommand<string?>(OnClick);
    }

    #region Events
    /// OnClickCommand
    private void OnClick(string? parameter)
    {
        if (int.TryParse(parameter, out var result))
            ClickOption = result;
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
    /// SelectedOption
    private ObservableCollection<bool?> selectedOption = new() { null, false, false, true, false };
    public ObservableCollection<bool?> SelectedOption
    {
        get => selectedOption;
        set => SetProperty(ref selectedOption, value);
    }

    /// IsThreeState
    private bool isThreeState = true;
    public bool IsThreeState
    {
        get => isThreeState;
        set => SetProperty(ref isThreeState, value);
    }
    #endregion
}
