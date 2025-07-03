using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestApp;

public partial class DatabasesContext : StswObservableObject
{
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand ExportCommand { get; }

    public DatabasesContext()
    {
        MoveUpCommand = new StswCommand(MoveUp);
        MoveDownCommand = new StswCommand(MoveDown);
        AddCommand = new StswCommand(Add);
        RemoveCommand = new StswCommand(Remove);
        ImportCommand = new StswAsyncCommand(Import);
        ExportCommand = new StswAsyncCommand(Export);

        SelectedDatabase = AllDatabases.FirstOrDefault() ?? new();
    }

    #region Commands & methods
    /// MoveUp
    private void MoveUp()
    {
        if (AllDatabases.IndexOf(SelectedDatabase!) is int i and > 0)
            AllDatabases.Move(i, i - 1);
    }

    /// MoveDown
    private void MoveDown()
    {
        if (AllDatabases.IndexOf(SelectedDatabase!) is int i and >= 0 && i < AllDatabases.Count - 1)
            AllDatabases.Move(i, i + 1);
    }

    /// Add
    private void Add()
    {
        var newDatabase = new StswDatabaseModel();
        AllDatabases.Add(newDatabase);
        SelectedDatabase = newDatabase;
    }

    /// Remove
    private void Remove()
    {
        if (SelectedDatabase != null)
            AllDatabases.Remove(SelectedDatabase);
        SelectedDatabase = null;
    }

    /// Import
    private async Task Import()
    {
        AllDatabases = [.. StswDatabases.ImportList()];
        await Task.Run(() => SQLService.DbCurrent = AllDatabases.FirstOrDefault() ?? new());
        SelectedDatabase = SQLService.DbCurrent;
    }

    /// Export
    private async Task Export()
    {
        await Task.Run(() => StswDatabases.ExportList(AllDatabases));
    }
    #endregion
    
    [StswObservableProperty] ObservableCollection<StswDatabaseModel> _allDatabases = [.. StswDatabases.ImportList()];
    [StswObservableProperty] StswDatabaseModel? _selectedDatabase;
}
