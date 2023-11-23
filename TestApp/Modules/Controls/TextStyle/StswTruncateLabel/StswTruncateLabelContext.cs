namespace TestApp;

public class StswTruncateLabelContext : ControlsContext
{
    #region Properties
    /// IsTruncationAllowed
    private bool isTruncationAllowed = true;
    public bool IsTruncationAllowed
    {
        get => isTruncationAllowed;
        set => SetProperty(ref isTruncationAllowed, value);
    }
    #endregion
}
