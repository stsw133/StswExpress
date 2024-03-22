using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TestApp;

public class StswIconContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => Scale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => Scale = new GridLength(1, GridUnitType.Star));

    public StswIconContext()
    {
        Task.Run(() => Icons = typeof(StswIcons).GetProperties()
                                                .Select(x => new StswComboItem() { Display = x.Name, Value = x.GetValue(x) })
                                                .OrderBy(x => x.Display)
                                                .ToList());
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Scale)))?.Value ?? default;
    }

    /// Data
    private Geometry? data = StswIcons.Abacus;
    public Geometry? Data
    {
        get => data;
        set => SetProperty(ref data, value);
    }

    /// Icons
    private List<StswComboItem> icons = new();
    public List<StswComboItem> Icons
    {
        get => icons;
        set => SetProperty(ref icons, value);
    }

    /// Scale
    private GridLength scale;
    public GridLength Scale
    {
        get => scale;
        set => SetProperty(ref scale, value);
    }
}
