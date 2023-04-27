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
    public StswProgressBar.States LoadingState => LoadingActions > 0 ? StswProgressBar.States.Running : StswProgressBar.States.Ready;

    /// Commands
    public StswRelayCommand ImportCommand { get; set; }
    public StswRelayCommand ExportCommand { get; set; }

    public DatabasesContext()
    {
        ImportCommand = new StswRelayCommand(Import);
        ExportCommand = new StswRelayCommand(Export);
    }

    /// Import
    private void Import()
    {
        LoadingActions++;
        StswDatabase.ImportDatabases();
        StswDatabase.CurrentDatabase = StswDatabase.AllDatabases.FirstOrDefault().Value ?? new();
        LoadingActions--;
    }

    /// Export
    private void Export()
    {
        LoadingActions++;
        StswDatabase.AllDatabases = new();
        StswDatabase.AllDatabases.Add(string.Empty, StswDatabase.CurrentDatabase);
        StswDatabase.ExportDatabases();
        LoadingActions--;
    }
}
