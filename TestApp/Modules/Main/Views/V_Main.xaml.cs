namespace TestApp.Modules.Main;

/// <summary>
/// Interaction logic for Main.xaml
/// </summary>
public partial class V_Main : StswWindow
{
    public V_Main()
    {
        InitializeComponent();
        DataContext = new D_Main();
    }
}
