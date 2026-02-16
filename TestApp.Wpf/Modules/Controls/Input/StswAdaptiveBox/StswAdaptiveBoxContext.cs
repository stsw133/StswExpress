using System.Collections.Generic;
using System.Linq;

namespace TestApp.Wpf;
public partial class StswAdaptiveBoxContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        Type = (StswAdaptiveType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    [StswCommand] void Clear() => SelectedValue = default;
    [StswCommand] void ClearType() => Type = null;

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] List<StswSelectableItem> _itemsSource = [new() { Value = "test1" }, new() { Value = "test2" }];
    [StswObservableProperty] object? _selectedValue;
    [StswObservableProperty] bool _subControls = false;
    [StswObservableProperty] StswAdaptiveType? _type;
}
