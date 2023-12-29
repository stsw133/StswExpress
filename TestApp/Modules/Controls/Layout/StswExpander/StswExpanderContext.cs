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
    private ExpandDirection expandDirection;
    public ExpandDirection ExpandDirection
    {
        get => expandDirection;
        set => SetProperty(ref expandDirection, value);
    }
}
