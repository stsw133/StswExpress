namespace TestApp;

public class StswToggleButtonContext : ControlsContext
{
    /// IsChecked
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
    private bool _isChecked;
}
