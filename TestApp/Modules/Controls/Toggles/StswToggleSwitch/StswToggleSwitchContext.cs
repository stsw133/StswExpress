using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;
public partial class StswToggleSwitchContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));
    
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        IsThreeState = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsThreeState)))?.Value ?? default;
    }

    [StswObservableProperty] int _clickOption;
    [StswObservableProperty] ObservableCollection<bool?> _selectedOption = [null, false, false, true, false];
    [StswObservableProperty] bool _hasContent = true;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] bool _isThreeState;
}
