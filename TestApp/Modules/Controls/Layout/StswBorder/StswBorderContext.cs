namespace TestApp;

public class StswBorderContext : ControlsContext
{
    /// ShowExampleContent
    private bool showExampleContent;
    public bool ShowExampleContent
    {
        get => showExampleContent;
        set => SetProperty(ref showExampleContent, value);
    }
}
