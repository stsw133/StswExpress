namespace TestApp;
public partial class ContractorsSingleContext : StswObservableObject
{
    [StswObservableProperty] int? _id = new();
    [StswObservableProperty] bool _isCloned = new();
}
