using System.Collections.Generic;

namespace TestApp;

public class StswSelectionBoxContext : ControlsContext
{
    #region Properties
    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

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
    #endregion
}
