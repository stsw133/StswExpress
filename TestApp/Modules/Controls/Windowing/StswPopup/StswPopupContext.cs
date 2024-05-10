namespace TestApp;

public class StswPopupContext : ControlsContext
{
    /// IsOpen
    public bool IsOpen
    {
        get => _isOpen;
        set => SetProperty(ref _isOpen, value);
    }
    private bool _isOpen = false;
}
