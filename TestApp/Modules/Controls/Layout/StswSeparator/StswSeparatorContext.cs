using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswSeparatorContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    #region Properties
    /// Orientation
    private Orientation orientation;
    public Orientation Orientation
    {
        get => orientation;
        set => SetProperty(ref orientation, value);
    }
    #endregion
}
