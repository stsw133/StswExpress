namespace TestApp;

public class StswDataGridContext : StswObservableObject
{
    public StswCollection<IStswCollectionItem> Items => new() { new ContractorModel(), new ContractorModel(), new ContractorModel() };
}
