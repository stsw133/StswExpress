using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswRatingControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Direction = (ExpandDirection?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Direction)))?.Value ?? default;
        ItemsNumber = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ItemsNumber)))?.Value ?? default;
        ItemsNumberVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ItemsNumberVisibility)))?.Value ?? default;
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

    /// ItemsNumberVisibility
    private Visibility itemsNumberVisibility;
    public Visibility ItemsNumberVisibility
    {
        get => itemsNumberVisibility;
        set => SetProperty(ref itemsNumberVisibility, value);
    }

    /// SelectedValue
    private double? selectedValue = 0;
    public double? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }
}
