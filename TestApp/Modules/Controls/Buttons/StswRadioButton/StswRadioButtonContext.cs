using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;
public partial class StswRadioButtonContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        VerticalAlignment = System.Windows.VerticalAlignment.Top;

        AllowUncheck = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AllowUncheck)))?.Value ?? default;
    }

    [StswCommand] void OnClick(string option) => ClickOption = Convert.ToInt32(option);

    [StswObservableProperty] int _clickOption;
    [StswObservableProperty] ObservableCollection<bool?> _selectedOption = [null, false, false, true, false];
    [StswObservableProperty] bool _allowUncheck;
}
