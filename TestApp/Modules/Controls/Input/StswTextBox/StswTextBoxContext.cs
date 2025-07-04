using System;
using System.Linq;

namespace TestApp;
public partial class StswTextBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => Text = string.Empty);
    public StswCommand RandomizeCommand => new(() => Text = Guid.NewGuid().ToString());

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] bool _subControls = false;
    [StswObservableProperty] string _text = string.Empty;
}
