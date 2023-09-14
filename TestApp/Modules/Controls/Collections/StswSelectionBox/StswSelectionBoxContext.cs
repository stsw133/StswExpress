using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace TestApp;

public class StswSelectionBoxContext : ControlsContext
{
    public ICommand ClearCommand { get; set; }
    public ICommand? SetTextCommand { get; set; }

    public StswSelectionBoxContext()
    {
        ClearCommand = new StswCommand(Clear);
        SetTextCommand = null;
    }

    #region Events and methods
    /// Command: clear
    private void Clear()
    {
        Items.Where(x => x.IsSelected).ToList().ForEach(x => x.IsSelected = false);
        SetTextCommand?.Execute(null);
    }
    #endregion

    #region Properties
    /// Components
    private bool components = false;
    public bool Components
    {
        get => components;
        set => SetProperty(ref components, value);
    }

    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Items
    private ObservableCollection<StswSelectionItem> items = new()
    {
        new() { Display = "Option 1", IsSelected = true },
        new() { Display = "Option 2", IsSelected = false },
        new() { Display = "Option 3", IsSelected = false },
        new() { Display = "Option 4", IsSelected = true },
        new() { Display = "Option 5", IsSelected = false },
        new() { Display = "Option 6", IsSelected = true },
        new() { Display = "Option 7", IsSelected = false },
        new() { Display = "Option 8", IsSelected = false },
        new() { Display = "Option 9", IsSelected = true },
        new() { Display = "Option 10", IsSelected = false }
    };
    public ObservableCollection<StswSelectionItem> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    #endregion
}
