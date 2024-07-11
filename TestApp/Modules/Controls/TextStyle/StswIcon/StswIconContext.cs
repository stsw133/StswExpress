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
        Task.Run(() => Icons = [.. typeof(StswIcons).GetProperties()
                                                    .Select(x => new StswComboItem() { Display = x.Name, Value = x.GetValue(x) })
                                                    .OrderBy(x => x.Display)]);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Scale)))?.Value ?? default;
    }

    /// Data
    public Geometry? Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }
    private Geometry? _data = StswIcons.Abacus;

    /// Icons
    public List<StswComboItem> Icons
    {
        get => _icons;
        set => SetProperty(ref _icons, value);
    }
    private List<StswComboItem> _icons = [];

    /// Scale
    public GridLength Scale
    {
        get => _scale;
        set => SetProperty(ref _scale, value);
    }
    private GridLength _scale;
}
