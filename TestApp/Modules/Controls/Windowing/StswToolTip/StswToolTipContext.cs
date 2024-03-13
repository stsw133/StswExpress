using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswToolTipContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsMoveable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsMoveable)))?.Value ?? default;
    }

    /// IsMoveable
    private bool isMoveable;
    public bool IsMoveable
    {
        get => isMoveable;
        set => SetProperty(ref isMoveable, value);
    }

    /// ShowDuration
    private int showDuration = ToolTipService.GetShowDuration(StswApp.StswWindow);
    public int ShowDuration
    {
        get => showDuration;
        set => SetProperty(ref showDuration, value);
    }
}
