using System.Linq;

namespace TestApp;
public partial class StswPathTreeContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        ShowFiles = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowFiles)))?.Value ?? default;
    }

    [StswObservableProperty] string? _initialPath;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] string? _selectedPath;
    [StswObservableProperty] bool _showFiles;
}
