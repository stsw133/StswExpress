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

    public V_Main()
    {
        InitializeComponent();
        DataContext = D;
    }

    /// Settings
    private void CmdSettings_Executed(object sender, ExecutedRoutedEventArgs e) => new Settings.V_Settings() { Owner = this }.ShowDialog();

    /// Refresh
    private async void CmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Cursor = Cursors.Wait;
        DtgUsers.GetColumnFilters(out var filter, out var parameters);
        await Task.Run(() =>
        {
            D.LoadingProgress = 0;

            /// Users
            D.ListUsers = Q_Main.GetListOfUsers(filter, parameters);
            D.LoadingProgress = 100;
        });
        Cursor = null;
    }

    /// Clear
    private async void CmdClear_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Cursor = Cursors.Wait;
        DtgUsers.ClearColumnFilters();
        await Task.Run(() =>
        {
            D.ListUsers = new();
        });
        Cursor = null;
    }

    /// Save
    private void CmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Cursor = Cursors.Wait;
        D.LoadingProgress = 0;
        if (Q_Main.SetListOfUsers(D.ListUsers))
            MessageBox.Show("Data saved successfully.");
        D.LoadingProgress = 100;
        Cursor = null;
    }
}
