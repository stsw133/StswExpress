using System.Linq;

namespace TestApp;

public class DatabasesContext : StswObservableObject
{
    /// Loading
    private int loadingActions = 0;
    public int LoadingActions
    {
        get => loadingActions;
        set
        {
            SetProperty(ref loadingActions, value);
            NotifyPropertyChanged(nameof(LoadingState));
        }
    }
    public StswProgressState LoadingState => LoadingActions > 0 ? StswProgressState.Running : StswProgressState.Ready;

    /// Commands
    public StswCommand ImportCommand { get; set; }
    public StswCommand ExportCommand { get; set; }

    public DatabasesContext()
    {
        ImportCommand = new StswCommand(Import);
        ExportCommand = new StswCommand(Export);
    }

    /// Import
    private void Import()
    {
        LoadingActions++;
        StswDatabase.ImportDatabases();
        StswDatabase.CurrentDatabase = StswDatabase.AllDatabases.FirstOrDefault() ?? new();
        LoadingActions--;
    }

    /// Export
    private void Export()
    {
        LoadingActions++;
        if (StswDatabase.CurrentDatabase != null && !StswDatabase.AllDatabases.Contains(StswDatabase.CurrentDatabase))
            StswDatabase.AllDatabases.Add(StswDatabase.CurrentDatabase);
        StswDatabase.ExportDatabases();
        LoadingActions--;
    }
}
