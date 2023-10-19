using System.Collections.ObjectModel;

namespace TestApp;

public class StswTreeViewContext : ControlsContext
{
    #region Properties
    /// Items
    private ObservableCollection<StswSelectionItem> items = new()
    {
        new() { Display = "Option 1", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 1a" } } },
        new() { Display = "Option 2", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 2a" } } },
        new() { Display = "Option 3", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 3a", IsSelected = true } } },
        new() { Display = "Option 4", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 4a" } } },
        new() { Display = "Option 5", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 5a" } } },
        new() { Display = "Option 6", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 6a" } } },
        new() { Display = "Option 7", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 7a" } } },
        new() { Display = "Option 8", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 8a" } } },
        new() { Display = "Option 9", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 9a" } } },
        new() { Display = "Option 10", Value = new ObservableCollection<StswSelectionItem>() { new() { Display = "Option 10a" } } }
    };
    public ObservableCollection<StswSelectionItem> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    #endregion
}
