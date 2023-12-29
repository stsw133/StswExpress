namespace TestApp;

public class StswToggleButtonContext : ControlsContext
{
    /// IsChecked
    private bool isChecked;
    public bool IsChecked
    {
        get => isChecked;
        set => SetProperty(ref isChecked, value);
    }
}
