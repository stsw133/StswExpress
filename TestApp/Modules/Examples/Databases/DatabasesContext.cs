using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestApp;

public class DatabasesContext : StswObservableObject
{
    public ICommand ImportCommand { get; set; }
    public ICommand ExportCommand { get; set; }

    public DatabasesContext()
    {
        ImportCommand = new StswAsyncCommand(Import);
        ExportCommand = new StswAsyncCommand(Export);
    }

    #region Commands
    /// Import
    private async Task Import()
    {
        LoadingActions++;

        await Task.Run(() =>
        {
            StswDatabase.ImportDatabases();
            StswDatabase.CurrentDatabase = StswDatabase.AllDatabases.FirstOrDefault() ?? new();
        });

        LoadingActions--;
    }

    /// Export
    private async Task Export()
    {
        LoadingActions++;

        await Task.Run(() =>
        {
            if (StswDatabase.CurrentDatabase != null && !StswDatabase.AllDatabases.Contains(StswDatabase.CurrentDatabase))
                StswDatabase.AllDatabases.Add(StswDatabase.CurrentDatabase);
            StswDatabase.ExportDatabases();
        });

        LoadingActions--;
    }
    #endregion

    #region Properties
    /// Loading
    private int loadingActions = 0;
    public int LoadingActions
    {
        get => loadingActions;
        set
        {
            SetProperty(ref loadingActions, value);

            if (!LoadingState.In(StswProgressState.Paused, StswProgressState.Error))
                LoadingState = LoadingActions > 0 ? StswProgressState.Running : StswProgressState.Ready;
        }
    }
    private StswProgressState loadingState;
    public StswProgressState LoadingState
    {
        get => loadingState;
        set => SetProperty(ref loadingState, value);
    }
    #endregion
}
