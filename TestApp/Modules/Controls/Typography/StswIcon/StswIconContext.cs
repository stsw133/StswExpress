using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TestApp;
public partial class StswIconContext : ControlsContext
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

    [StswObservableProperty] Geometry? _data = StswIcons.Abacus;
    [StswObservableProperty] IReadOnlyList<StswComboItem> _icons = [];
    [StswObservableProperty] GridLength _scale;
}
