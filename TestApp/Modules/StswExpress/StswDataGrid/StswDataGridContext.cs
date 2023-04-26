namespace TestApp;

public class StswDataGridContext : StswObservableObject
{
    public StswCollection<StswCollectionItem> Items => new() { new(), new(), new() };
}
