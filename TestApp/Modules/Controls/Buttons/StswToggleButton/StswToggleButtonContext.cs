namespace TestApp;

public class StswToggleButtonContext : ControlsContext
{
    #region Properties
    /// IsChecked
    private bool isChecked;
    public bool IsChecked
    {
        get => isChecked;
        set => SetProperty(ref isChecked, value);
    }
    #endregion
}
