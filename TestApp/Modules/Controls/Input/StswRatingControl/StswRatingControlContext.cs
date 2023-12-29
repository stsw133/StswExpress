using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswRatingControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Direction = (ExpandDirection?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Direction)))?.Value ?? default;
        ItemsNumber = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ItemsNumber)))?.Value ?? default;
    }

    /// Direction
    private ExpandDirection direction;
    public ExpandDirection Direction
    {
        get => direction;
        set => SetProperty(ref direction, value);
    }

    /// ItemsNumber
    private int itemsNumber;
    public int ItemsNumber
    {
        get => itemsNumber;
        set => SetProperty(ref itemsNumber, value);
    }

    /// SelectedValue
    private double? selectedValue = 0;
    public double? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }
}
