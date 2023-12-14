using System;
using System.Collections.ObjectModel;

namespace TestApp;

public class StswRadioButtonContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));

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
