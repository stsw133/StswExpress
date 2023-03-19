namespace TestApp;

public class StswProgressBarContext : StswObservableObject
{
    private double number = 0;
    public double Number
    {
        get => number;
        set => SetProperty(ref number, value);
    }

    private double number2 = 100;
    public double Number2
    {
        get => number2;
        set => SetProperty(ref number2, value);
    }
}
