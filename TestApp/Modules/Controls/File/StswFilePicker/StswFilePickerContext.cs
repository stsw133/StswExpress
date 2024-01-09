using System.Linq;

namespace TestApp;

public class StswFilePickerContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedPath = default);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        IsShiftingEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsShiftingEnabled)))?.Value ?? default;
        PathType = (StswPathType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(PathType)))?.Value ?? default;
    }

    /// Components
    private bool components = false;
    public bool Components
    {
        get => components;
        set => SetProperty(ref components, value);
    }

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }
    
    /// IsShiftingEnabled
    private bool isShiftingEnabled;
    public bool IsShiftingEnabled
    {
        get => isShiftingEnabled;
        set => SetProperty(ref isShiftingEnabled, value);
    }

    /// PathType
    private StswPathType? pathType;
    public StswPathType? PathType
    {
        get => pathType;
        set => SetProperty(ref pathType, value);
    }

    /// SelectedPath
    private string? selectedPath;
    public string? SelectedPath
    {
        get => selectedPath;
        set => SetProperty(ref selectedPath, value);
    }
}
