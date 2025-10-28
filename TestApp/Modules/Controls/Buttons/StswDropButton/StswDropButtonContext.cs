using System;
using System.Linq;

namespace TestApp;
public partial class StswDropButtonContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        VerticalAlignment = System.Windows.VerticalAlignment.Top;

        AutoClose = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AutoClose)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswCommand] void OnClick(string option) => ClickOption = Convert.ToInt32(option);

    [StswObservableProperty] bool _autoClose;
    [StswObservableProperty] int _clickOption;
    [StswObservableProperty] bool _isReadOnly;
}
