using System.Linq;

namespace TestApp;
public partial class StswPathPickerContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedPath = default);

    public override void SetDefaults()
    {
        base.SetDefaults();

        Filter = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Filter)))?.Value ?? default;
        IsFileSizeVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsFileSizeVisible)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        IsShiftingEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsShiftingEnabled)))?.Value ?? default;
        Multiselect = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Multiselect)))?.Value ?? default;
        SelectionUnit = (StswPathType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionUnit)))?.Value ?? default;
        SuggestedFilename = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SuggestedFilename)))?.Value ?? default;
    }

    [StswObservableProperty] string? _filter;
    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isFileSizeVisible;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] bool _isShiftingEnabled;
    [StswObservableProperty] bool _multiselect;
    [StswObservableProperty] string? _selectedPath;
    [StswObservableProperty] StswPathType? _selectionUnit;
    [StswObservableProperty] bool _subControls = false;
    [StswObservableProperty] string? _suggestedFilename;
}
