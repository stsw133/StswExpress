namespace TestApp;

public class StswLabelContext : ControlsContext
{
    #region Properties
    /// IsTruncationAllowed
    private bool isTruncationAllowed = false;
    public bool IsTruncationAllowed
    {
        get => isTruncationAllowed;
        set => SetProperty(ref isTruncationAllowed, value);
    }
    #endregion
}
