using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp.Wpf;
public partial class DatabasesContext : StswObservableObject
{
    public DatabasesContext()
    {
        SelectedDatabase = AllDatabases.FirstOrDefault();
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task MoveUp()
    {
        if (AllDatabases.IndexOf(SelectedDatabase!) is int i and > 0)
            AllDatabases.Move(i, i - 1);
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task MoveDown()
    {
        if (AllDatabases.IndexOf(SelectedDatabase!) is int i and >= 0 && i < AllDatabases.Count - 1)
            AllDatabases.Move(i, i + 1);
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Add()
    {
        var newDatabase = new StswDatabaseModel();
        AllDatabases.Add(newDatabase);
        SelectedDatabase = newDatabase;
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Remove()
    {
        if (SelectedDatabase != null)
            AllDatabases.Remove(SelectedDatabase);
        SelectedDatabase = null;
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Import()
    {
        AllDatabases = new(await Task.Run(StswDatabases.ImportList));
        if (AllDatabases.FirstOrDefault() is StswDatabaseModel db)
            SQLService.DbCurrent = db;
        SelectedDatabase = SQLService.DbCurrent;
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Export()
    {
        await Task.Run(() => StswDatabases.ExportList(AllDatabases));
    }

    [StswObservableProperty] ObservableCollection<StswDatabaseModel> _allDatabases = [.. StswDatabases.ImportList()];
    [StswObservableProperty] StswDatabaseModel? _selectedDatabase;
}
