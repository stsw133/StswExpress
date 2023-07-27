using System.Collections.Generic;
using System.Windows.Input;

namespace TestApp;

public class StswSelectionBoxContext : ControlsContext
{
    public ICommand ClearCommand { get; set; }

    public StswSelectionBoxContext()
    {
        ClearCommand = new StswCommand(Clear);
    }

    #region Events
    /// Command: clear
    private void Clear() => SelectedItems = new();
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
