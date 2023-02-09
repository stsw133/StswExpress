namespace TestApp.Modules.ContractorsSingle;

public class ContractorsSingleContext : StswContext
{
    /// ID
    private int? id = new();
    public int? ID
    {
        get => id;
        set => SetProperty(ref id, value, () => ID);
    }
}
