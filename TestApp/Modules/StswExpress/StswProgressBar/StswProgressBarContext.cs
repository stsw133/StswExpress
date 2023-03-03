namespace TestApp;

public class StswProgressBarContext : StswObservableObject
{
    private double number = 0;
    public double Number
    {
        get => number;
        set => SetProperty(ref number, value);
    }
}
