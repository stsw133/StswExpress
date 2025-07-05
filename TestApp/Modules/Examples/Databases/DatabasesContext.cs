using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp;
public partial class DatabasesContext : StswObservableObject
{
    public DatabasesContext()
    {
        SelectedDatabase = AllDatabases.FirstOrDefault() ?? new();
        InitializeGeneratedCommands();
    }

    [StswCommand] void MoveUp()
    {
        if (AllDatabases.IndexOf(SelectedDatabase!) is int i and > 0)
            AllDatabases.Move(i, i - 1);
    }
    [StswCommand] void MoveDown()
    {
        if (AllDatabases.IndexOf(SelectedDatabase!) is int i and >= 0 && i < AllDatabases.Count - 1)
            AllDatabases.Move(i, i + 1);
    }
    [StswCommand] void Add()
    {
        var newDatabase = new StswDatabaseModel();
        AllDatabases.Add(newDatabase);
        SelectedDatabase = newDatabase;
    }
    [StswCommand] void Remove()
    {
        if (SelectedDatabase != null)
            AllDatabases.Remove(SelectedDatabase);
        SelectedDatabase = null;
    }
    [StswAsyncCommand] async Task Import()
    {
        AllDatabases = [.. StswDatabases.ImportList()];
        await Task.Run(() => SQLService.DbCurrent = AllDatabases.FirstOrDefault() ?? new());
        SelectedDatabase = SQLService.DbCurrent;
    }
    [StswAsyncCommand] async Task Export()
    {
        await Task.Run(() => StswDatabases.ExportList(AllDatabases));
    }
    
    [StswObservableProperty] ObservableCollection<StswDatabaseModel> _allDatabases = [.. StswDatabases.ImportList()];
    [StswObservableProperty] StswDatabaseModel? _selectedDatabase;
}
