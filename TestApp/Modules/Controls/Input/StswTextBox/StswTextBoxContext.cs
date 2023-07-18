namespace TestApp;

public class StswTextBoxContext : ControlsContext
{
    #region Properties
    /// Text
    private string text = string.Empty;
    public string Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }
    #endregion
}
