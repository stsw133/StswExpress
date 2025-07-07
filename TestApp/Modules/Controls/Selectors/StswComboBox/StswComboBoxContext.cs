using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace TestApp;
public partial class StswComboBoxContext : ControlsContext
{
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

    [StswCommand] void Randomize() => SelectedItem = Items[new Random().Next(Items.Count)];

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isEditable;
    [StswObservableProperty] bool _isFilterEnabled;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] List<string> _items = [.. Enumerable.Range(1, 3000).Select(i => "Option " + i)];
    [StswObservableProperty] ICollectionView? _itemsCollectionView;
    private readonly CollectionViewSource itemsCollectionViewSource;
    [StswObservableProperty] string? _selectedItem;
    [StswObservableProperty] bool _subControls = false;
}
