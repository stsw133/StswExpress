namespace TestApp.Wpf;
public partial class StswToggleButtonContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        VerticalAlignment = System.Windows.VerticalAlignment.Top;
    }

    [StswObservableProperty] bool _isChecked;
}
