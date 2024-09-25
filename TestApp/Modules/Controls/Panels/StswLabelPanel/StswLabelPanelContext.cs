using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswLabelPanelContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => LabelWidth = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => LabelWidth = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        LabelFontWeight = (FontWeight?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(LabelFontWeight)))?.Value ?? default;
        LabelHorizontalAlignment = (HorizontalAlignment?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(LabelHorizontalAlignment)))?.Value ?? default;
        LabelWidth = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(LabelWidth)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    /// LabelFontWeight
    public FontWeight LabelFontWeight
    {
        get => _labelFontWeight;
        set => SetProperty(ref _labelFontWeight, value);
    }
    private FontWeight _labelFontWeight;

    /// LabelHorizontalAlignment
    public HorizontalAlignment LabelHorizontalAlignment
    {
        get => _labelHorizontalAlignment;
        set => SetProperty(ref _labelHorizontalAlignment, value);
    }
    private HorizontalAlignment _labelHorizontalAlignment;

    /// LabelWidth
    public GridLength LabelWidth
    {
        get => _labelWidth;
        set => SetProperty(ref _labelWidth, value);
    }
    private GridLength _labelWidth;

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;
}
