using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswHeaderContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
        IsHighlighted = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsHighlighted)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    /// IconScale
    private GridLength iconScale;
    public GridLength IconScale
    {
        get => iconScale;
        set => SetProperty(ref iconScale, value);
    }

    /// IsBusy
    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }

    /// IsContentVisible
    private bool isContentVisible;
    public bool IsContentVisible
    {
        get => isContentVisible;
        set => SetProperty(ref isContentVisible, value);
    }

    /// IsHighlighted
    private bool isHighlighted;
    public bool IsHighlighted
    {
        get => isHighlighted;
        set => SetProperty(ref isHighlighted, value);
    }

    /// Orientation
    private Orientation orientation;
    public Orientation Orientation
    {
        get => orientation;
        set => SetProperty(ref orientation, value);
    }

    /// ShowDescription
    private bool showDescription = false;
    public bool ShowDescription
    {
        get => showDescription;
        set => SetProperty(ref showDescription, value);
    }
}
