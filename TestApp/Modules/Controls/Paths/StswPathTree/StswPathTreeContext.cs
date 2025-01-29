using System.Linq;

namespace TestApp;

public class StswPathTreeContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        ShowFiles = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowFiles)))?.Value ?? default;
    }

    /// InitialPath
    public string? InitialPath
    {
        get => _initialPath;
        set => SetProperty(ref _initialPath, value);
    }
    private string? _initialPath;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// SelectedPath
    public string? SelectedPath
    {
        get => _selectedPath;
        set => SetProperty(ref _selectedPath, value);
    }
    private string? _selectedPath;

    /// ShowFiles
    public bool ShowFiles
    {
        get => _showFiles;
        set => SetProperty(ref _showFiles, value);
    }
    private bool _showFiles;
}
