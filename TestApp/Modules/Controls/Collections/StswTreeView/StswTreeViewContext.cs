using System.Collections.Generic;

namespace TestApp;

public class StswTreeViewContext : ControlsContext
{
    #region Properties
    /// Items
    private List<StswTreeItem> items = new()
    {
        new() { Display = "Option 1", SubItems = new List<StswTreeItem>() { new() { Display = "Option 1a" } } },
        new() { Display = "Option 2", SubItems = new List<StswTreeItem>() { new() { Display = "Option 2a" } } },
        new() { Display = "Option 3", SubItems = new List<StswTreeItem>() { new() { Display = "Option 3a" } } },
        new() { Display = "Option 4", SubItems = new List<StswTreeItem>() { new() { Display = "Option 4a" } } },
        new() { Display = "Option 5", SubItems = new List<StswTreeItem>() { new() { Display = "Option 5a" } } },
        new() { Display = "Option 6", SubItems = new List<StswTreeItem>() { new() { Display = "Option 6a" } } },
        new() { Display = "Option 7", SubItems = new List<StswTreeItem>() { new() { Display = "Option 7a" } } },
        new() { Display = "Option 8", SubItems = new List<StswTreeItem>() { new() { Display = "Option 8a" } } },
        new() { Display = "Option 9", SubItems = new List<StswTreeItem>() { new() { Display = "Option 9a" } } },
        new() { Display = "Option 10", SubItems = new List<StswTreeItem>() { new() { Display = "Option 10a" } } }
    };
    public List<StswTreeItem> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    #endregion
}

public class StswTreeItem : StswSelectionItem
{
    public List<StswTreeItem> SubItems { get; set; } = new();
}
