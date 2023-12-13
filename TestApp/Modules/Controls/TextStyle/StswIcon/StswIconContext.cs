using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace TestApp;

public class StswIconContext : ControlsContext
{
    #region Properties
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
    private double scale = 2;
    public double Scale
    {
        get => scale;
        set => SetProperty(ref scale, value);
    }
    #endregion
}
