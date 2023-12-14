using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp;

public class StswComboBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedItem = null);
    public StswCommand RandomizeCommand => new(() => SelectedItem = Items[new Random().Next(0, Items.Count)]);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsEditable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsEditable)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    #region Properties
    /// Components
    private bool components = false;
    public bool Components
    {
        get => components;
        set => SetProperty(ref components, value);
    }

    /// IsEditable
    private bool isEditable;
    public bool IsEditable
    {
        get => isEditable;
        set => SetProperty(ref isEditable, value);
    }

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

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
