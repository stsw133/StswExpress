using System;
using System.Linq;

namespace TestApp;
public partial class StswRichBoxContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswCommand] void Clear() => Text = string.Empty;
    [StswCommand] void Randomize() => Text = Guid.NewGuid().ToString();

    [StswObservableProperty] string? _filePath;
    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] bool _subControls = false;
    [StswObservableProperty] string _text = string.Empty;
}
