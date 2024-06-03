namespace TestApp;

public class StswBorderContext : ControlsContext
{
    /// ShowExampleContent
    public bool ShowExampleContent
    {
        get => _showExampleContent;
        set => SetProperty(ref _showExampleContent, value);
    }
    private bool _showExampleContent;
}
