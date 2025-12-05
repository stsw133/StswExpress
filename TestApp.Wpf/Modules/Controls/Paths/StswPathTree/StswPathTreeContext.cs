using System.Linq;

namespace TestApp.Wpf;
public partial class StswPathTreeContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Filter = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Filter)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        ShowFiles = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowFiles)))?.Value ?? default;
    }

    [StswObservableProperty] string? _filter;
    [StswObservableProperty] string? _initialPath;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] string? _selectedPath;
    [StswObservableProperty] bool _showFiles;
}
