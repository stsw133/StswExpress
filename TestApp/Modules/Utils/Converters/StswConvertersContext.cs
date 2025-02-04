namespace TestApp;
public class StswConvertersContext : StswObservableObject
{
    /// Values
    public StswObservableDictionary<string, object?> Values
    {
        get => _values;
        set => SetProperty(ref _values, value);
    }
    private StswObservableDictionary<string, object?> _values = [];

    /// Parameters
    public StswObservableDictionary<string, object?> Parameters
    {
        get => _parameters;
        set => SetProperty(ref _parameters, value);
    }
    private StswObservableDictionary<string, object?> _parameters = [];
}
