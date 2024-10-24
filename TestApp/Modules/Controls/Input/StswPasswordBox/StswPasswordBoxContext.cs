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

    /// Icon
    public bool Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }
    private bool _icon;

    /// Password
    public string? Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    private string? _password;

    /// ShowPassword
    public bool ShowPassword
    {
        get => _showPassword;
        set => SetProperty(ref _showPassword, value);
    }
    private bool _showPassword = false;

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;
}
