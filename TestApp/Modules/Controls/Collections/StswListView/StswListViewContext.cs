using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswListViewContext : ControlsContext
{
    public StswListViewContext()
    {
        Items.ListChanged += (s, e) => NotifyPropertyChanged(nameof(SelectionCounter));
    }

    #region Properties
    /// Items
    private StswBindingList<StswListBoxTestModel> items = new()
    {
        new() { Name = "Option 1", IsSelected = true },
        new() { Name = "Option 2", IsSelected = false },
        new() { Name = "Option 3", IsSelected = false },
        new() { Name = "Option 4", IsSelected = true },
        new() { Name = "Option 5", IsSelected = false },
        new() { Name = "Option 6", IsSelected = true },
        new() { Name = "Option 7", IsSelected = false },
        new() { Name = "Option 8", IsSelected = false },
        new() { Name = "Option 9", IsSelected = true },
        new() { Name = "Option 10", IsSelected = false }
    };
    public StswBindingList<StswListBoxTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SelectionCounter
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);

    /// SelectionMode
    private SelectionMode selectionMode = SelectionMode.Multiple;
    public SelectionMode SelectionMode
    {
        get => selectionMode;
        set => SetProperty(ref selectionMode, value);
    }
    #endregion
}
