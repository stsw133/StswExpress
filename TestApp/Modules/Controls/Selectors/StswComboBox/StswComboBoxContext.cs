using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TestApp;
public partial class StswComboBoxContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        SelectedItem = Items[new Random().Next(Items.Count)];
        IsEditable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.IsEditableProperty)?.Value ?? default;
        IsFilterEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.IsFilterEnabledProperty)?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.IsReadOnlyProperty)?.Value ?? default;
        DropArrowVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property == StswDropArrow.VisibilityProperty)?.Value ?? default;
    }

    [StswCommand] void Randomize() => SelectedItem = Items[new Random().Next(Items.Count)];

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isEditable;
    [StswObservableProperty] bool _isFilterEnabled;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] List<string> _items = [.. Enumerable.Range(1, 3000).Select(i => "Option " + i)];
    [StswObservableProperty] string? _selectedItem;
    [StswObservableProperty] bool _subControls = false;
    [StswObservableProperty] Visibility _dropArrowVisibility = Visibility.Visible;
}
