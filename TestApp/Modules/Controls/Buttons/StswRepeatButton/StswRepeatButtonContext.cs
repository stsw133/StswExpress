namespace TestApp;

public class StswRepeatButtonContext : ControlsContext
{
    public StswCommand OnClickCommand => new(() => ClickCounter++);

    /// ClickCounter
    public int ClickCounter
    {
        get => _clickCounter;
        set => SetProperty(ref _clickCounter, value);
    }
    private int _clickCounter;
}
