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

        SelectedItem = Items[new Random().Next(30)];
        IsEditable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsEditable)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

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
    private List<string> items = Enumerable.Range(1, 30).Select(i => "Option " + i).ToList();
    public List<string> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SelectedItem
    private string? selectedItem;
    public string? SelectedItem
    {
        get => selectedItem;
        set => SetProperty(ref selectedItem, value);
    }
}
