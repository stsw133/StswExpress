namespace TestApp;

public class StswPopupContext : ControlsContext
{
    #region Properties
    /// IsOpen
    private bool isOpen;
    public bool IsOpen
    {
        get => isOpen;
        set => SetProperty(ref isOpen, value);
    }
    #endregion
}
