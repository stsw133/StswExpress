using System.Linq;

namespace TestApp;

public class StswFilePickerContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedPath = default);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsFileSizeVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsFileSizeVisible)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        IsShiftingEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsShiftingEnabled)))?.Value ?? default;
        PathType = (StswPathType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(PathType)))?.Value ?? default;
    }

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

    /// PathType
    public StswPathType? PathType
    {
        get => _pathType;
        set => SetProperty(ref _pathType, value);
    }
    private StswPathType? _pathType;

    /// SelectedPath
    public string? SelectedPath
    {
        get => _selectedPath;
        set => SetProperty(ref _selectedPath, value);
    }
    private string? _selectedPath;

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;
}
