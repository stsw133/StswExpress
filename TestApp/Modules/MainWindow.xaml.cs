using System.Reflection;

namespace TestApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : StswWindow
{
    public MainWindow()
    {
        InitializeComponent();

        var v = Assembly.GetAssembly(typeof(StswWindow))?.GetName().Version!;
        Title = $"{nameof(TestApp)} {v.Major}.{v.Minor}.{v.Build}";
    }
}
