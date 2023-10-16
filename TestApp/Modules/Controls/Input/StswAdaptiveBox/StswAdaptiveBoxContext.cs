namespace TestApp;

public class StswAdaptiveBoxContext : ControlsContext
{
    public StswCommand ClearTypeCommand { get; set; }

    public StswAdaptiveBoxContext()
    {
        ClearTypeCommand = new(ClearType);
    }

    #region Events and methods
    /// Command: clear Type
    private void ClearType() => Type = null;
    #endregion

    #region Properties
    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// SelectedValue
    private object? selectedValue;
    public object? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }

    /// Type
    private StswAdaptiveType? type;
    public StswAdaptiveType? Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }
    #endregion
}
