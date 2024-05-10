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
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;

    /// Text
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
    private string _text = string.Empty;
}
