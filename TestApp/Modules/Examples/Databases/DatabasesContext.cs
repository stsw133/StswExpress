using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TestApp;
public partial class DatabasesContext : StswObservableObject
{
    public DatabasesContext()
    {
        SelectedDatabase = AllDatabases.FirstOrDefault();
    }

    [StswCommand]
    async Task MoveUp()
    {
        try
        {
            if (AllDatabases.IndexOf(SelectedDatabase!) is int i and > 0)
                AllDatabases.Move(i, i - 1);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task MoveDown()
    {
        try
        {
            if (AllDatabases.IndexOf(SelectedDatabase!) is int i and >= 0 && i < AllDatabases.Count - 1)
                AllDatabases.Move(i, i + 1);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task Add()
    {
        try
        {
            var newDatabase = new StswDatabaseModel();
            AllDatabases.Add(newDatabase);
            SelectedDatabase = newDatabase;
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task Remove()
    {
        try
        {
            if (SelectedDatabase != null)
                AllDatabases.Remove(SelectedDatabase);
            SelectedDatabase = null;

        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task Import()
    {
        try
        {
            AllDatabases = new(await Task.Run(StswDatabases.ImportList));
            if (AllDatabases.FirstOrDefault() is StswDatabaseModel db)
                SQLService.DbCurrent = db;
            SelectedDatabase = SQLService.DbCurrent;
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task Export()
    {
        try
        {
            await Task.Run(() => StswDatabases.ExportList(AllDatabases));
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswObservableProperty] ObservableCollection<StswDatabaseModel> _allDatabases = [.. StswDatabases.ImportList()];
    [StswObservableProperty] StswDatabaseModel? _selectedDatabase;
}
