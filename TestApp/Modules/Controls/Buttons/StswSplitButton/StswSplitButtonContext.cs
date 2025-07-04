using System;
using System.Linq;

namespace TestApp;
public partial class StswSplitButtonContext : ControlsContext
{
    public StswCommand<string> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));

    public override void SetDefaults()
    {
        base.SetDefaults();

        AutoClose = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AutoClose)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswObservableProperty] bool _autoClose;
    [StswObservableProperty] int _clickOption;
    [StswObservableProperty] bool _isReadOnly;
}
