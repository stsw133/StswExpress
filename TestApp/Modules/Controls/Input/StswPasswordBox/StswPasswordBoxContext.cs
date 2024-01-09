using System;
using System.Linq;

namespace TestApp;

public class StswPasswordBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => Password = string.Empty);
    public StswCommand RandomizeCommand => new(() => Password = Guid.NewGuid().ToString());

    public override void SetDefaults()
    {
        base.SetDefaults();

        ShowPassword = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowPassword)))?.Value ?? default;
    }

    /// Password
    private string? password;
    public string? Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    /// ShowPassword
    private bool showPassword = false;
    public bool ShowPassword
    {
        get => showPassword;
        set => SetProperty(ref showPassword, value);
    }

    /// SubControls
    private bool subControls = false;
    public bool SubControls
    {
        get => subControls;
        set => SetProperty(ref subControls, value);
    }
}
