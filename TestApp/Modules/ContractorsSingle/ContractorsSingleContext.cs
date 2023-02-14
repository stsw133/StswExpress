namespace TestApp.Modules.ContractorsSingle;

public class ContractorsSingleContext : StswObservableObject
{
    /// ID
    private int? id = new();
    public int? ID
    {
        get => id;
        set => SetProperty(ref id, value);
    }
}
