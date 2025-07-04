namespace TestApp;
public partial class StswRepeatButtonContext : ControlsContext
{
    [StswCommand] void OnClick() => ClickCounter++;

    [StswObservableProperty] int _clickCounter;
}
