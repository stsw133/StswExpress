using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TestApp;

public class StswDropArrowContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => Scale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => Scale = new GridLength(1, GridUnitType.Star));

    public StswDropArrowContext()
    {
        Task.Run(() => Icons = [.. typeof(StswIcons).GetProperties()
                                 .Select(x => new StswComboItem() { Display = x.Name, Value = x.GetValue(x) })
                                 .OrderBy(x => x.Display)]);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Data = (Geometry?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Data)))?.Value ?? default;
        IsExpanded = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsExpanded)))?.Value ?? default;
        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Scale)))?.Value ?? default;
    }

    /// Data
    public Geometry? Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }
    private Geometry? _data;

    /// Icons
    public IReadOnlyList<StswComboItem> Icons
    {
        get => _icons;
        set => SetProperty(ref _icons, value);
    }
    private IReadOnlyList<StswComboItem> _icons = [];

    /// IsExpanded
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
    private bool _isExpanded;

    /// Scale
    public GridLength Scale
    {
        get => _scale;
        set => SetProperty(ref _scale, value);
    }
    private GridLength _scale;
}
