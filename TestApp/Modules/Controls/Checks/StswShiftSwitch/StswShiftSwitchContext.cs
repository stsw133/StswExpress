namespace TestApp;

public class StswShiftSwitchContext : ControlsContext
{
    /// IsChecked
    private bool? isChecked;
    public bool? IsChecked
    {
        get => isChecked;
        set => SetProperty(ref isChecked, value);
    }

    /// SelectedIndex
    private int selectedIndex;
    public int SelectedIndex
    {
        get => selectedIndex;
        set => SetProperty(ref selectedIndex, value);
    }
}
