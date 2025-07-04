using System;
using System.Collections.ObjectModel;

namespace TestApp;
public partial class StswRadioButtonContext : ControlsContext
{
    public StswCommand<string> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));

    [StswObservableProperty] int _clickOption;
    [StswObservableProperty] ObservableCollection<bool?> _selectedOption = [null, false, false, true, false];
}
