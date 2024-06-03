using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace TestApp;

public class StswComboBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedItem = null);
    public StswCommand RandomizeCommand => new(() => SelectedItem = Items[new Random().Next(0, Items.Count)]);

    public StswComboBoxContext()
    {
        itemsCollectionViewSource = new CollectionViewSource() { Source = Items };
        ItemsCollectionView = itemsCollectionViewSource.View;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        SelectedItem = Items[new Random().Next(Items.Count)];
        IsEditable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsEditable)))?.Value ?? default;
        IsFilterEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsFilterEnabled)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// IsEditable
    public bool IsEditable
    {
        get => _isEditable;
        set => SetProperty(ref _isEditable, value);
    }
    private bool _isEditable;

    /// IsFilterEnabled
    public bool IsFilterEnabled
    {
        get => _isFilterEnabled;
        set => SetProperty(ref _isFilterEnabled, value);
    }
    private bool _isFilterEnabled;

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
    private List<string> _items = Enumerable.Range(1, 3000).Select(i => "Option " + i).ToList();

    /// ItemsCollectionView
    public ICollectionView? ItemsCollectionView
    {
        get => _itemsCollectionView;
        set => SetProperty(ref _itemsCollectionView, value);
    }
    private ICollectionView? _itemsCollectionView;
    private CollectionViewSource itemsCollectionViewSource;

    /// SelectedItem
    public string? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }
    private string? _selectedItem;

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;
}
