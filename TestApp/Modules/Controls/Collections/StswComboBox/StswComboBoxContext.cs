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
    private bool isEditable;
    public bool IsEditable
    {
        get => isEditable;
        set => SetProperty(ref isEditable, value);
    }

    /// IsFilterEnabled
    private bool isFilterEnabled;
    public bool IsFilterEnabled
    {
        get => isFilterEnabled;
        set => SetProperty(ref isFilterEnabled, value);
    }

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Items
    private List<string> items = Enumerable.Range(1, 3000).Select(i => "Option " + i).ToList();
    public List<string> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// ItemsCollectionView
    private ICollectionView? itemsCollectionView;
    public ICollectionView? ItemsCollectionView
    {
        get => itemsCollectionView;
        set => SetProperty(ref itemsCollectionView, value);
    }
    private CollectionViewSource itemsCollectionViewSource;

    /// SelectedItem
    private string? selectedItem;
    public string? SelectedItem
    {
        get => selectedItem;
        set => SetProperty(ref selectedItem, value);
    }

    /// SubControls
    private bool subControls = false;
    public bool SubControls
    {
        get => subControls;
        set => SetProperty(ref subControls, value);
    }
}
