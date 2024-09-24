using System;
using System.Linq;

namespace TestApp;

public class StswSplitButtonContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));

    public override void SetDefaults()
    {
        base.SetDefaults();

        AutoClose = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AutoClose)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// AutoClose
    public bool AutoClose
    {
        get => _autoClose;
        set => SetProperty(ref _autoClose, value);
    }
    private bool _autoClose;

    /// ClickOption
    public int ClickOption
    {
        get => _clickOption;
        set => SetProperty(ref _clickOption, value);
    }
    private int _clickOption;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;
}
