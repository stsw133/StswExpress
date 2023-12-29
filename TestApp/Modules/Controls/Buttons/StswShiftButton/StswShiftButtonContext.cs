using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp;

public class StswShiftButtonContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        SelectedItem = Items[new Random().Next(30)];
        IsEditable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsEditable)))?.Value ?? default;
        IsLoopingEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsLoopingEnabled)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// IsEditable
    private bool isEditable;
    public bool IsEditable
    {
        get => isEditable;
        set => SetProperty(ref isEditable, value);
    }

    /// IsLoopingEnabled
    private bool isLoopingEnabled;
    public bool IsLoopingEnabled
    {
        get => isLoopingEnabled;
        set => SetProperty(ref isLoopingEnabled, value);
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
