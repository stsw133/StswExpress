using System.Linq;

namespace TestApp;

public class StswPathTreeContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ShowFiles = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowFiles)))?.Value ?? default;
    }

    /// InitialPath
    public string? InitialPath
    {
        get => _initialPath;
        set => SetProperty(ref _initialPath, value);
    }
    private string? _initialPath;
    
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
