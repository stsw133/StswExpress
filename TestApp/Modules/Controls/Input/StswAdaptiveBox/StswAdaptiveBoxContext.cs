using System.Collections.Generic;
using System.Linq;

namespace TestApp;

public class StswAdaptiveBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedValue = default);
    public StswCommand ClearTypeCommand => new(() => Type = null);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        Type = (StswAdaptiveType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    /// Icon
    public bool Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }
    private bool _icon;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// ItemsSource
    public List<StswSelectionItem> ItemsSource
    {
        get => _itemsSource;
        set => SetProperty(ref _itemsSource, value);
    }
    private List<StswSelectionItem> _itemsSource = [new() { Value = "test1" }, new() { Value = "test2" }];

    /// SelectedValue
    public object? SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }
    private object? _selectedValue;

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;

    /// Type
    public StswAdaptiveType? Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    private StswAdaptiveType? _type;
}
