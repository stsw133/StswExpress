using System.Windows.Controls;

namespace TestApp;

public class StswToolTipContext : ControlsContext
{
    /// ShowDuration
    private int showDuration = ToolTipService.GetShowDuration(StswApp.StswWindow);
    public int ShowDuration
    {
        get => showDuration;
        set => SetProperty(ref showDuration, value);
    }
}
