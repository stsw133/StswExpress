using System.Linq;

namespace TestApp;

public class StswPathPickerContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedPath = default);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsFileSizeVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsFileSizeVisible)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        IsShiftingEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsShiftingEnabled)))?.Value ?? default;
        SelectionUnit = (StswPathType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionUnit)))?.Value ?? default;
    }

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
}
