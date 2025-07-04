namespace TestApp;
public partial class StswRepeatButtonContext : ControlsContext
{
    public StswCommand OnClickCommand => new(() => ClickCounter++);

    [StswObservableProperty] int _clickCounter;
}
