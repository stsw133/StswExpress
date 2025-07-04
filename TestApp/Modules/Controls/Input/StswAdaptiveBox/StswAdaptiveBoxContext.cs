using System.Collections.Generic;
using System.Linq;

namespace TestApp;
public partial class StswAdaptiveBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedValue = default);
    public StswCommand ClearTypeCommand => new(() => Type = null);
    
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        Type = (StswAdaptiveType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] List<StswSelectionItem> _itemsSource = [new() { Value = "test1" }, new() { Value = "test2" }];
    [StswObservableProperty] object? _selectedValue;
    [StswObservableProperty] bool _subControls = false;
    [StswObservableProperty] StswAdaptiveType? _type;
}
