using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswExpanderContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ExpandDirection = (ExpandDirection?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ExpandDirection)))?.Value ?? default;
    }

    /// ExpandDirection
    public ExpandDirection ExpandDirection
    {
        get => _expandDirection;
        set => SetProperty(ref _expandDirection, value);
    }
    private ExpandDirection _expandDirection;
}
