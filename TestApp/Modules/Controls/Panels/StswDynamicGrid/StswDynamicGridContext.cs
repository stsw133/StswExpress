using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswDynamicGridContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Columns = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Columns)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
        Rows = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Rows)))?.Value ?? default;
        Spacing = (double?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Spacing)))?.Value ?? default;
        StretchColumnIndex = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(StretchColumnIndex)))?.Value ?? default;
        StretchRowIndex = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(StretchRowIndex)))?.Value ?? default;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
    }

    /// Columns
    public int Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }
    private int _columns;

    /// Orientation
    public Orientation? Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation? _orientation;

    /// Rows
    public int Rows
    {
        get => _rows;
        set => SetProperty(ref _rows, value);
    }
    private int _rows;

    /// Spacing
    public double Spacing
    {
        get => _spacing;
        set => SetProperty(ref _spacing, value);
    }
    private double _spacing;

    /// StretchColumnIndex
    public int StretchColumnIndex
    {
        get => _stretchColumnIndex;
        set => SetProperty(ref _stretchColumnIndex, value);
    }
    private int _stretchColumnIndex;

    /// StretchRowIndex
    public int StretchRowIndex
    {
        get => _stretchRowIndex;
        set => SetProperty(ref _stretchRowIndex, value);
    }
    private int _stretchRowIndex;
}
