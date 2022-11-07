﻿using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TestApp.Modules.Settings;

namespace TestApp.Modules.Main;

/// <summary>
/// Interaction logic for Main.xaml
/// </summary>
public partial class V_Main : StswWindow
{
    private readonly D_Settings D = new D_Settings();
    private Button? BtnSettings;

    public V_Main()
    {
        InitializeComponent();
        DataContext = D;
    }

    /// Window_Loaded
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BtnSettings = AddButtonToTitleBar("🔧");
        if (BtnSettings != null)
            BtnSettings.Command = Commands.Settings;
    }

    /// Settings
    private void CmdSettings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Cursor = Cursors.Wait;
        new V_Settings() { Owner = this }.ShowDialog();
        Cursor = null;
    }

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
