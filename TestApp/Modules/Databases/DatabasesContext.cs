using System.Collections.Generic;
using System.Linq;

namespace TestApp.Modules.Databases;

public class DatabasesContext : StswContext
{
    /// CurrentDB
    private StswDB? currentDB;
    public StswDB? CurrentDB
    {
        get => currentDB;
        set => SetProperty(ref currentDB, value, () => CurrentDB);
    }

    /// IsLoading
    private int countActions = 0;
    public int CountActions
    {
        get => countActions;
        set
        {
            SetProperty(ref countActions, value, () => CountActions);
            NotifyPropertyChanged(nameof(IsLoading));
        }
    }
    public bool IsLoading => CountActions > 0;

    /// Commands
    public StswRelayCommand ImportCommand { get; set; }
    public StswRelayCommand ExportCommand { get; set; }

    public DatabasesContext()
    {
        ImportCommand = new StswRelayCommand(o => Import(), o => true);
        ExportCommand = new StswRelayCommand(o => Export(), o => true);

        Import();
    }

    /// Import
    private void Import()
    {
        CountActions++;
        CurrentDB = StswDB.ImportDatabases().FirstOrDefault() ?? new();
        CountActions--;
    }

    /// Export
    private void Export()
    {
        CountActions++;
        StswDB.ExportDatabases(new List<StswDB>() { CurrentDB });
        CountActions--;
    }
}
