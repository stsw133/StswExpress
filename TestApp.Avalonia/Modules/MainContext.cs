namespace TestApp.Ava;
public partial class MainContext : StswObservableObject
{
    public string CountText => $"Licznik: {_count}";
    private int _count;
    
    [StswCommand] void IncrementCount()
    {
        _count++;
        OnPropertyChanged(nameof(CountText));
    }
}
