using System;
using System.Linq;

namespace TestApp;

public class StswTextBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => Text = string.Empty);
    public StswCommand RandomizeCommand => new(() => Text = Guid.NewGuid().ToString());

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// SubControls
    private bool subControls = false;
    public bool SubControls
    {
        get => subControls;
        set => SetProperty(ref subControls, value);
    }

    /// Text
    private string text = string.Empty;
    public string Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }
}
