using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswToolTipContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsMoveable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsMoveable)))?.Value ?? default;
    }

    [StswObservableProperty] bool _isMoveable;
    [StswObservableProperty] int _showDuration = ToolTipService.GetShowDuration(StswApp.StswWindow);
}
