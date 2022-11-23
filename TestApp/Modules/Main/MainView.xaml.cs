namespace TestApp.Modules.Main;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : StswWindow
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainContext(this);
    }
}
