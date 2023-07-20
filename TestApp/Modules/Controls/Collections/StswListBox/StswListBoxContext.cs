using System.Collections.Generic;
using System.Windows.Controls;

namespace TestApp;

public class StswListBoxContext : ControlsContext
{
    #region Properties
    /// Items
    private List<string?> items = new() { "Option 1", "Option 2", "Option 3", "Option 4", "Option 5" };
    public List<string?> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    /// SelectedItems
    private List<string?> selectedItems = new() { "Option 1", "Option 4" };
    public List<string?> SelectedItems
    {
        get => selectedItems;
        set
        {
            SetProperty(ref selectedItems, value);
            SelectedItemsCount = selectedItems.Count;
        }
    }
    /// SelectedItemsCount
    private int selectedItemsCount = 2;
    public int SelectedItemsCount
    {
        get => selectedItemsCount;
        set => SetProperty(ref selectedItemsCount, value);
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
