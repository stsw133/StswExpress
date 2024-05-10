namespace TestApp;

public class ContractorsSingleContext : StswObservableObject
{
    /// ID
    public int? ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    private int? _id = new();

    /// IsCloned
    public bool IsCloned
    {
        get => _isCloned;
        set => SetProperty(ref _isCloned, value);
    }
    private bool _isCloned = new();
}
