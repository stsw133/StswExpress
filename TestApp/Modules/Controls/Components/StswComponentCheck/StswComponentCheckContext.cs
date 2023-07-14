namespace TestApp;

public class StswComponentCheckContext : ControlsContext
{
    #region Properties
    /// IsThreeState
    private bool isThreeState = false;
    public bool IsThreeState
    {
        get => isThreeState;
        set => SetProperty(ref isThreeState, value);
    }
    #endregion
}
