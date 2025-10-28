namespace TestApp;
public partial class StswRepeatButtonContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        VerticalAlignment = System.Windows.VerticalAlignment.Top;
    }

    [StswCommand] void OnClick() => ClickCounter++;

    [StswObservableProperty] int _clickCounter;
}
