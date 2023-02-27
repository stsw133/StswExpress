namespace TestApp;

public class ContractorsSingleContext : StswObservableObject
{
    /// ID
    private int? id = new();
    public int? ID
    {
        get => id;
        set => SetProperty(ref id, value);
    }
    /// DoClone
    private bool doClone = new();
    public bool DoClone
    {
        get => doClone;
        set => SetProperty(ref doClone, value);
    }
}
