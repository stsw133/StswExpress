using System.Collections.Generic;
using System.Linq;

namespace TestApp.Modules.Databases;

public class DatabasesContext : StswObservableObject
{
    /// CurrentDB
    private StswDB? currentDB;
    public StswDB? CurrentDB
    {
        get => currentDB;
        set => SetProperty(ref currentDB, value);
    }

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

        Import();
    }

    /// Import
    private void Import()
    {
        LoadingActions++;
        StswFn.AppDB = CurrentDB = StswDB.ImportDatabases().FirstOrDefault() ?? new();
        LoadingActions--;
    }

    /// Export
    private void Export()
    {
        LoadingActions++;
        StswDB.ExportDatabases(new List<StswDB>() { CurrentDB });
        LoadingActions--;
    }
}
