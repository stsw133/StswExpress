using System;

namespace TestApp;

public class StswSidePanelContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));

    /// ClickOption
    public int ClickOption
    {
        get => _clickOption;
        set => SetProperty(ref _clickOption, value);
    }
    private int _clickOption;
}
