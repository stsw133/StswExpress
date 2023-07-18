namespace TestApp;

public class StswPasswordBoxContext : ControlsContext
{
    #region Properties
    /// Password
    private string password = "TEST";
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }
    #endregion
}
