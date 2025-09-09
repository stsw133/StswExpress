using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;
public partial class StswRadioBoxContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        AllowUncheck = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AllowUncheck)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        IsThreeState = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsThreeState)))?.Value ?? default;
    }

    [StswCommand] void OnClick(string option) => ClickOption = Convert.ToInt32(option);

    [StswObservableProperty] int _clickOption;
    [StswObservableProperty] ObservableCollection<bool?> _selectedOption = [null, false, false, true, false];
    [StswObservableProperty] bool _allowUncheck;
    [StswObservableProperty] bool _hasContent = true;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] bool _isThreeState;
}
