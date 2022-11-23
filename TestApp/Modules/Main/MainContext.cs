using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestApp.Modules.Main;

public class MainContext : BaseContext
{
    public MainContext(Window window)
    {
        window.CommandBindings.Add(new CommandBinding(Commands.Clear, CmdClear_Executed));
        window.CommandBindings.Add(new CommandBinding(Commands.Refresh, CmdRefresh_Executed));
        window.CommandBindings.Add(new CommandBinding(Commands.Save, CmdSave_Executed));
        window.CommandBindings.Add(new CommandBinding(Commands.Settings, CmdSettings_Executed));
        //CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(Commands.Settings, CmdSettings_Executed));
    }

    #region Data
    /// ColumnFilters
    private ExtDictionary<string, ColumnFilter> columnFilters = new();
    public ExtDictionary<string, ColumnFilter> ColumnFilters
    {
        get => columnFilters;
        set => SetField(ref columnFilters, value, () => ColumnFilters);
    }

    /// LoadingProgress
    private double loadingProgress = 0;
    public double LoadingProgress
    {
        get => loadingProgress;
        set => SetField(ref loadingProgress, value, () => LoadingProgress);
    }

    /// ComboLists
    public List<string?> ListTypes => MainQueries.ListOfTypes();

    /// ListUsers
    private ExtCollection<UserModel> listUsers = new();
    public ExtCollection<UserModel> ListUsers
    {
        get => listUsers;
        set => SetField(ref listUsers, value, () => ListUsers);
    }
    #endregion

    #region Events
    /// Clear
    private async void CmdClear_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        ColumnFilters.ClearColumnFilters();
        await Task.Run(() =>
        {
            ListUsers = new();
        });
    }

    /// Refresh
    private async void CmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        ColumnFilters.GetColumnFilters(out var filter, out var parameters);
        await Task.Run(() =>
        {
            LoadingProgress = 0;

            /// Users
            ListUsers = MainQueries.GetListOfUsers(filter, parameters);
            LoadingProgress = 100;
        });
    }

    /// Save
    private void CmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        LoadingProgress = 0;
        if (MainQueries.SetListOfUsers(ListUsers))
            MessageBox.Show("Data saved successfully.");
        LoadingProgress = 100;
    }

    /// Settings
    private void CmdSettings_Executed(object sender, ExecutedRoutedEventArgs e) => new Settings.SettingsView() { Owner = Application.Current.MainWindow }.ShowDialog();
    #endregion
}
