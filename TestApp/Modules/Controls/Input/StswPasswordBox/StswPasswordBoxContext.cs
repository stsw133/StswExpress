using System;
using System.Linq;

namespace TestApp;
public partial class StswPasswordBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => Password = string.Empty);
    public StswCommand RandomizeCommand => new(() => Password = Guid.NewGuid().ToString());

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        ShowPassword = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowPassword)))?.Value ?? default;
    }

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] string? _password;
    [StswObservableProperty] bool _showPassword;
    [StswObservableProperty] bool _subControls;
}
