using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace TestApp;

public class StswListBoxContext : ControlsContext
{
    #region Properties
    /// Items
    private ObservableCollection<StswComboItem> items = new()
    {
        new() { Display = "Option 1", IsSelected = true },
        new() { Display = "Option 2", IsSelected = false },
        new() { Display = "Option 3", IsSelected = false },
        new() { Display = "Option 4", IsSelected = true },
        new() { Display = "Option 5", IsSelected = false }
    };
    public ObservableCollection<StswComboItem> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SelectionMode
    private SelectionMode selectionMode = SelectionMode.Multiple;
    public SelectionMode SelectionMode
    {
        get => selectionMode;
        set => SetProperty(ref selectionMode, value);
    }
    #endregion
}
