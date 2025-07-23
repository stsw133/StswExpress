using System;
using System.Collections.ObjectModel;

namespace TestApp;
public partial class StswRadioButtonContext : ControlsContext
{
    [StswCommand] void OnClick(string option) => ClickOption = Convert.ToInt32(option);

    [StswObservableProperty] int _clickOption;
    [StswObservableProperty] ObservableCollection<bool?> _selectedOption = [null, false, false, true, false];
}
