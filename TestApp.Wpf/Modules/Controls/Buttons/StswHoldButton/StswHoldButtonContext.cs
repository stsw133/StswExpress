namespace TestApp.Wpf;
public partial class StswHoldButtonContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        VerticalAlignment = System.Windows.VerticalAlignment.Top;
    }

    [StswCommand] void OnClick() => ClickCounter++;

    [StswObservableProperty] int _clickCounter;
    [StswObservableProperty] bool _isDefault;
}
