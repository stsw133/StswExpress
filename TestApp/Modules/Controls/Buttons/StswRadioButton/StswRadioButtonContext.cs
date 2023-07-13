using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TestApp;

public class StswRadioButtonContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswRadioButtonContext()
    {
        OnClickCommand = new StswRelayCommand(OnClick);
    }

    #region Events
    /// OnClickCommand
    private void OnClick() => ClickOption = SelectedOption.IndexOf(true) + 1;
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
    #endregion
}
