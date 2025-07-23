using System;
using System.Linq;

namespace TestApp;
public partial class StswPasswordBoxContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        ShowPassword = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowPassword)))?.Value ?? default;
    }

    [StswCommand] void Clear() => Password = string.Empty;
    [StswCommand] void Randomize() => Password = Guid.NewGuid().ToString();

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] string? _password;
    [StswObservableProperty] bool _showPassword;
    [StswObservableProperty] bool _subControls;
}
