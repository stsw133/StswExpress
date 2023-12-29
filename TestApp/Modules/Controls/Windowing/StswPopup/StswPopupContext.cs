namespace TestApp;

public class StswPopupContext : ControlsContext
{
    /// IsOpen
    private bool isOpen = false;
    public bool IsOpen
    {
        get => isOpen;
        set => SetProperty(ref isOpen, value);
    }
}
