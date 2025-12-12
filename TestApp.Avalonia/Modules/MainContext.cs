namespace TestApp.Ava;
public class MainContext : StswObservableObject
{
    public string CountText => $"Licznik: {_count}";
    private int _count;

    public void Increment()
    {
        _count++;
        OnPropertyChanged(nameof(CountText));
    }
}
