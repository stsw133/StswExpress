using System.Linq;

namespace TestApp;

public class StswPathPickerContext : ControlsContext
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

    /// Filter
    public string? Filter
    {
        get => _filter;
        set => SetProperty(ref _filter, value);
    }
    private string? _filter;

    /// Icon
    public bool Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }
    private bool _icon;

    /// IsFileSizeVisible
    public bool IsFileSizeVisible
    {
        get => _isFileSizeVisible;
        set => SetProperty(ref _isFileSizeVisible, value);
    }
    private bool _isFileSizeVisible;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;
    
    /// IsShiftingEnabled
    public bool IsShiftingEnabled
    {
        get => _isShiftingEnabled;
        set => SetProperty(ref _isShiftingEnabled, value);
    }
    private bool _isShiftingEnabled;

    /// Multiselect
    public bool Multiselect
    {
        get => _multiselect;
        set => SetProperty(ref _multiselect, value);
    }
    private bool _multiselect;

    /// SelectedPath
    public string? SelectedPath
    {
        get => _selectedPath;
        set => SetProperty(ref _selectedPath, value);
    }
    private string? _selectedPath;

    /// SelectionUnit
    public StswPathType? SelectionUnit
    {
        get => _selectionUnit;
        set => SetProperty(ref _selectionUnit, value);
    }
    private StswPathType? _selectionUnit;

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;

    /// SuggestedFilename
    public string? SuggestedFilename
    {
        get => _suggestedFilename;
        set => SetProperty(ref _suggestedFilename, value);
    }
    private string? _suggestedFilename;
}
