namespace TestApp.Wpf;
public partial class StswConvertersContext : StswObservableObject
{
    [StswObservableProperty] StswObservableDictionary<string, object?> _values = [];
    [StswObservableProperty] StswObservableDictionary<string, object?> _parameters = [];
}
