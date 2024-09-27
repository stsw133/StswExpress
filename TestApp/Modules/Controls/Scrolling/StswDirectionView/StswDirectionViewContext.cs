using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswDirectionViewContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;
}
