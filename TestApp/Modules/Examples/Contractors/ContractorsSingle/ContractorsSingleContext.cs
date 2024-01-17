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

    /// IsCloned
    private bool isCloned = new();
    public bool IsCloned
    {
        get => isCloned;
        set => SetProperty(ref isCloned, value);
    }
}
