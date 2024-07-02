namespace TestApp;
public class StswConvertersContext : StswObservableObject
{
    /// Values
    public StswDictionary<string, object> Values
    {
        get => _values;
        set => SetProperty(ref _values, value);
    }
    private StswDictionary<string, object> _values = [];

    /// Parameters
    public StswDictionary<string, object> Parameters
    {
        get => _parameters;
        set => SetProperty(ref _parameters, value);
    }
    private StswDictionary<string, object> _parameters = [];
}
