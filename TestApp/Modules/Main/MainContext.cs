using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestApp.Modules.Main;

public class MainContext : StswContext
{
    public MainContext()
    {
        App.Current.MainWindow.CommandBindings.Add(new CommandBinding(StswCommands.Clear, CmdClear_Executed));
        App.Current.MainWindow.CommandBindings.Add(new CommandBinding(StswCommands.Refresh, CmdRefresh_Executed));
        App.Current.MainWindow.CommandBindings.Add(new CommandBinding(StswCommands.Save, CmdSave_Executed));
        App.Current.MainWindow.CommandBindings.Add(new CommandBinding(StswCommands.Settings, CmdSettings_Executed));
        //CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(StswCommands.Settings, CmdSettings_Executed));
    }
    
    #region Data
    /// ColumnFilters
    private ExtDictionary<string, ColumnFilter> columnFilters = new();
    public ExtDictionary<string, ColumnFilter> ColumnFilters
    {
        get => columnFilters;
        set => SetProperty(ref columnFilters, value, () => ColumnFilters);
    }

    /// LoadingProgress
    private double loadingProgress = 0;
    public double LoadingProgress
    {
        get => loadingProgress;
        set => SetProperty(ref loadingProgress, value, () => LoadingProgress);
    }

    /// ComboLists
    public List<string?> ListTypes => MainQueries.ListOfTypes();

    /// ListUsers
    private ExtCollection<UserModel> listUsers = new();
    public ExtCollection<UserModel> ListUsers
    {
        get => listUsers;
        set => SetProperty(ref listUsers, value, () => ListUsers);
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
