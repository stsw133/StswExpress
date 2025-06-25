using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a base data model for chart elements, containing values, descriptions, colors, and interactive properties.
/// </summary>
public class StswChartElementModel : StswObservableObject
{
    /// <summary>
    /// Gets or sets the brush used to represent the chart item.
    /// </summary>
    public Brush? Brush
    {
        get => _brush;
        set => SetProperty(ref _brush, value);
    }
    private Brush? _brush;

    /// <summary>
    /// Gets or sets the description of the chart item.
    /// </summary>
    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
    private string? _description;

    /// <summary>
    /// Gets or sets the internal chart data. Type of data depends on chart type.
    /// </summary>
    public object? Internal
    {
        get => _internal;
        internal set => SetProperty(ref _internal, value);
    }
    private object? _internal;

    /// <summary>
    /// Gets or sets the name of the chart item.
    /// </summary>
    public string? Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value);
            Brush ??= new SolidColorBrush(StswFnUI.GenerateColor(Name ?? string.Empty, 130));
        }
    }
    private string? _name;

    /// <summary>
    /// Gets or sets the percentage value of the chart item.
    /// </summary>
    public double Percentage
    {
        get => _percentage;
        internal set => SetProperty(ref _percentage, value);
    }
    private double _percentage;

    /// <summary>
    /// Gets or sets the value of the chart item.
    /// </summary>
    public decimal Value
    {
        get => _value;
        set
        {
            SetProperty(ref _value, value);
            OnValueChangedCommand?.Execute(null);
        }
    }
    private decimal _value;

    /// <summary>
    /// Gets or sets the commmand that executes whether the value is changed.
    /// </summary>
    public ICommand? OnValueChangedCommand;
}

/// <summary>
/// Data model for chart items.
/// </summary>
internal class StswChartElementColumnModel : StswObservableObject
{
    /// <summary>
    /// Gets or sets the column height of the chart item.
    /// </summary>
    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }
    private double _height;

    /// <summary>
    /// Gets or sets the column width of the chart item.
    /// </summary>
    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }
    private double _width;
}

/// <summary>
/// Data model for chart items.
/// </summary>
internal class StswChartElementPieModel : StswObservableObject
{
    /// <summary>
    /// Gets or sets the angle of the chart item.
    /// </summary>
    public double Angle
    {
        get => _angle;
        set => SetProperty(ref _angle, value);
    }
    private double _angle;

    /// <summary>
    /// Gets or sets the center point of the chart item.
    /// </summary>
    public Point Center
    {
        get => _center;
        set => SetProperty(ref _center, value);
    }
    private Point _center;

    /// <summary>
    /// Gets or sets the visibility of the chart item's percentage.
    /// </summary>
    public bool IsPercentageVisible
    {
        get => _isPercentageVisible;
        set => SetProperty(ref _isPercentageVisible, value);
    }
    private bool _isPercentageVisible;

    /// <summary>
    /// Gets or sets the stroke dash array used to represent the chart item.
    /// </summary>
    public DoubleCollection? StrokeDashArray
    {
        get => _strokeDashArray;
        set => SetProperty(ref _strokeDashArray, value);
    }
    private DoubleCollection? _strokeDashArray;

    /// <summary>
    /// Gets or sets the size of the chart item's text.
    /// </summary>
    public double TextSize
    {
        get => _textSize;
        set => SetProperty(ref _textSize, value);
    }
    private double _textSize;
}
