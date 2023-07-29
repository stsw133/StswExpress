using System.Windows.Input;
using System;

namespace TestApp;

public class StswPasswordBoxContext : ControlsContext
{
    public ICommand RandomizeCommand { get; set; }

    public StswPasswordBoxContext()
    {
        RandomizeCommand = new StswCommand(Randomize);
    }

    #region Events and methods
    /// Command: randomize
    private void Randomize() => Password = Guid.NewGuid().ToString();
    #endregion

    #region Properties
    /// Components
    private bool components = false;
    public bool Components
    {
        get => components;
        set => SetProperty(ref components, value);
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
    #endregion
}
