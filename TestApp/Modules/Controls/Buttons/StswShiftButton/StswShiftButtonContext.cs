using System.Collections.Generic;

namespace TestApp;

public class StswShiftButtonContext : ControlsContext
{
    #region Properties
    /// Items
    private List<string?> items = new()
    {
        "Option 1",
        "Option 2",
        "Option 3",
        "Option 4",
        "Option 5",
        "Option 6",
        "Option 7",
        "Option 8",
        "Option 9",
        "Option 10"
    };
    public List<string?> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    /// SelectedItem
    private string? selectedItem = "Option 4";
    public string? SelectedItem
    {
        get => selectedItem;
        set => SetProperty(ref selectedItem, value);
    }
    #endregion
}
