using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TestApp.Wpf;
public partial class StswComboBoxContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        SelectedItem = Items[new Random().Next(Items.Count)];

        ClearFilterOnDropDownOpen = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.ClearFilterOnDropDownOpenProperty)?.Value ?? default;
        HideSelectedItemWhenFiltered = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.HideSelectedItemWhenFilteredProperty)?.Value ?? default;
        IsEditable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.IsEditableProperty)?.Value ?? default;
        IsFilterEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.IsFilterEnabledProperty)?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.IsReadOnlyProperty)?.Value ?? default;
        DropArrowVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property == StswDropArrow.VisibilityProperty)?.Value ?? default;
    }
    
    [StswCommand] void Randomize() => SelectedItem = Items[new Random().Next(Items.Count)];

    [StswObservableProperty] bool _clearFilterOnDropDownOpen;
    [StswObservableProperty] bool _hideSelectedItemWhenFiltered;
    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isEditable;
    [StswObservableProperty] bool _isFilterEnabled;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] List<string> _items = [.. Enumerable.Range(1, 3000).Select(i => "Option " + i)];
    [StswObservableProperty] string? _selectedItem;
    [StswObservableProperty] bool _subControls;
    [StswObservableProperty] Visibility _dropArrowVisibility;
}
