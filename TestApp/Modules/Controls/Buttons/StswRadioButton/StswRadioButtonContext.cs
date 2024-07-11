using System;
using System.Collections.ObjectModel;

namespace TestApp;

public class StswRadioButtonContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));

    /// ClickOption
    public int ClickOption
    {
        get => _clickOption;
        set => SetProperty(ref _clickOption, value);
    }
    private int _clickOption;

    /// SelectedOption
    public ObservableCollection<bool?> SelectedOption
    {
        get => _selectedOption;
        set => SetProperty(ref _selectedOption, value);
    }
    private ObservableCollection<bool?> _selectedOption = [null, false, false, true, false];
}
