using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestApp.Modules.Main;

public class D_Main : BaseD
{
    #region Data
    /// SelectedTab
    public int SelectedTab { get; set; }

    /// ColumnFilters
    private ExtDictionary<string, ColumnFilterModel> columnFilters = new();
    public ExtDictionary<string, ColumnFilterModel> ColumnFilters
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
    public List<string?> ListTypes => Q_Main.ListOfTypes();

    /// ListUsers
    private ExtCollection<M_User> listUsers = new();
    public ExtCollection<M_User> ListUsers
    {
        get => listUsers;
        set => SetField(ref listUsers, value, () => ListUsers);
    }
    #endregion

    #region Events
    public D_Main()
    {
        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(Commands.Clear, CmdClear_Executed));
        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(Commands.Refresh, CmdRefresh_Executed));
        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(Commands.Save, CmdSave_Executed));
        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(Commands.Settings, CmdSettings_Executed));
    }

    /// Clear
    private async void CmdClear_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        switch (SelectedTab)
        {
            case 0:
                ColumnFilters.ClearColumnFilters();
                await Task.Run(() =>
                {
                    ListUsers = new();
                });
                break;
        }
    }

    /// Refresh
    private async void CmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        switch (SelectedTab)
        {
            case 0:
                ColumnFilters.GetColumnFilters(out var filter, out var parameters);
                await Task.Run(() =>
                {
                    LoadingProgress = 0;

                    /// Users
                    ListUsers = Q_Main.GetListOfUsers(filter, parameters);
                    LoadingProgress = 100;
                });
                break;
        }
    }

    /// Save
    private void CmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        switch (SelectedTab)
        {
            case 0:
                LoadingProgress = 0;
                if (Q_Main.SetListOfUsers(ListUsers))
                    MessageBox.Show("Data saved successfully.");
                LoadingProgress = 100;
                break;
        }
    }

    /// Settings
    private void CmdSettings_Executed(object sender, ExecutedRoutedEventArgs e) => new Settings.V_Settings() { Owner = Application.Current.MainWindow }.ShowDialog();
    #endregion
}
