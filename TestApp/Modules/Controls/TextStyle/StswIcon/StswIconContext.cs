using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TestApp;

public class StswIconContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => Scale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => Scale = new GridLength(1, GridUnitType.Star));

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
    public List<StswComboItem> Icons => typeof(StswIcons).GetProperties()
                                                         .Select(x => new StswComboItem() { Display = x.Name, Value = x.GetValue(x) })
                                                         .OrderBy(x => x.Display)
                                                         .ToList();

    /// Scale
    private GridLength scale;
    public GridLength Scale
    {
        get => scale;
        set => SetProperty(ref scale, value);
    }
}
