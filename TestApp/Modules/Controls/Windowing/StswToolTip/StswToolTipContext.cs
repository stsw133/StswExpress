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
    public bool IsMoveable
    {
        get => _isMoveable;
        set => SetProperty(ref _isMoveable, value);
    }
    private bool _isMoveable;

    /// ShowDuration
    public int ShowDuration
    {
        get => _showDuration;
        set => SetProperty(ref _showDuration, value);
    }
    private int _showDuration = ToolTipService.GetShowDuration(StswApp.StswWindow);
}
