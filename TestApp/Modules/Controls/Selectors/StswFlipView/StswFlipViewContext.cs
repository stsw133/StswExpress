using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp;

public class StswFlipViewContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        SelectedItem = Items[new Random().Next(30)];
        IsLoopingEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsLoopingEnabled)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// IsLoopingEnabled
    public bool IsLoopingEnabled
    {
        get => _isLoopingEnabled;
        set => SetProperty(ref _isLoopingEnabled, value);
    }
    private bool _isLoopingEnabled;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// Items
    public List<string> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private List<string> _items = Enumerable.Range(1, 30).Select(i => "Option " + i).ToList();

    /// SelectedItem
    public string? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }
    private string? _selectedItem;
}
