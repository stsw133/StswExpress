using StswExpress;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TestApp.Modules.Main;
/// <summary>
/// Interaction logic for Main.xaml
/// </summary>
public partial class V_Main : StswWindow
{
    private readonly D_Main D = new D_Main();
    private Button? BtnSettings;

    public V_Main()
    {
        InitializeComponent();
        DataContext = D;
    }

    /// Window_Loaded
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BtnSettings = AddButtonToTitleBar("⚙");
        if (BtnSettings != null)
            BtnSettings.Command = Commands.Settings;
    }

    /// Settings
    private void CmdSettings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Cursor = Cursors.Wait;
        //TODO - open settings window (mainly DB config)
        Cursor = null;
    }

    /// UpdateFilters
    private void UpdateFilters()
    {
        D.LoadingProgress = 0;
        Fn.GetColumnFilters(DtgTest, out var filter, out var parameters);
        D.FilterSqlString = filter;
        D.FilterSqlParams = parameters;
    }

    /// Refresh
    private async void CmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Cursor = Cursors.Wait;
        UpdateFilters();
        await Task.Run(() =>
        {
            D.ListTest = Q_Main.GetListTest(D.FilterSqlString, D.FilterSqlParams);
            D.LoadingProgress = 100;
        });
        Cursor = null;
    }

    /// Clear
    private async void CmdClear_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Cursor = Cursors.Wait;
        Fn.ClearColumnFilters(DtgTest);
        await Task.Run(() =>
        {
            D.ListTest = new();
        });
        Cursor = null;
    }

    /// Save
    private void CmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Cursor = Cursors.Wait;
        //TODO - saving data from DtgTest to SQL
        Cursor = null;
    }
}
