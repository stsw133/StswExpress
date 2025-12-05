namespace TestApp.Wpf;
public partial class ContractorsSingleContext : StswObservableObject
{
    [StswObservableProperty] int? _id = 0;
    [StswObservableProperty] bool _isCloned;
}
