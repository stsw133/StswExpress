namespace TestApp;

public class StswRepeatButtonContext : ControlsContext
{
    public StswCommand OnClickCommand => new(() => ClickCounter++);

    /// ClickCounter
    private int clickCounter;
    public int ClickCounter
    {
        get => clickCounter;
        set => SetProperty(ref clickCounter, value);
    }
}
