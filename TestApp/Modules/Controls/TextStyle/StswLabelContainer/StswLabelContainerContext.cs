using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswLabelContainerContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        HeaderFontWeight = (FontWeight?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HeaderFontWeight)))?.Value ?? default;
        HeaderOrientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HeaderOrientation)))?.Value ?? default;
        HeaderWidth = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HeaderWidth)))?.Value ?? double.NaN;
        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    /// HeaderFontWeight
    public FontWeight HeaderFontWeight
    {
        get => _headerFontWeight;
        set => SetProperty(ref _headerFontWeight, value);
    }
    private FontWeight _headerFontWeight;

    /// HeaderOrientation
    public Orientation HeaderOrientation
    {
        get => _headerOrientation;
        set => SetProperty(ref _headerOrientation, value);
    }
    private Orientation _headerOrientation;

    /// HeaderWidth
    public double HeaderWidth
    {
        get => _headerWidth;
        set => SetProperty(ref _headerWidth, value);
    }
    private double _headerWidth;

    /// IconScale
    public GridLength IconScale
    {
        get => _iconScale;
        set => SetProperty(ref _iconScale, value);
    }
    private GridLength _iconScale;

    /// IsBusy
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;

    /// IsContentVisible
    public bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetProperty(ref _isContentVisible, value);
    }
    private bool _isContentVisible;

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;
}
