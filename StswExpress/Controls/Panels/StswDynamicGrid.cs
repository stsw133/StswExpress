using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// A dynamic panel that arranges its children in a flexible grid-like structure.
/// It supports automatic layout based on the number of items, customizable spacing, and stretching specific rows or columns.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswGrid Columns="2" StretchColumnIndex="1"&gt;
///     &lt;se:StswLabel Content="Header 1:" HorizontalContentAlignment="Right"/&gt;
///     &lt;se:StswTextBox Text="{Binding Text1}"/&gt;
///     &lt;se:StswLabel Content="Header 2:" HorizontalContentAlignment="Right"/&gt;
///     &lt;se:StswTextBox Text="{Binding Text2}"/&gt;
/// &lt;/se:StswGrid&gt;
/// </code>
/// </example>
public class StswDynamicGrid : Panel
{
    #region Events & methods
    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var itemCount = InternalChildren.Count;
        Orientation effectiveOrientation = GetEffectiveOrientation();
        (var columns, var rows) = CalculateGridSize(itemCount, effectiveOrientation);

        var columnWidths = new double[columns];
        var rowHeights = new double[rows];

        for (var i = 0; i < itemCount; i++)
        {
            var child = InternalChildren[i];
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var col = effectiveOrientation == System.Windows.Controls.Orientation.Horizontal ? i % columns : i / rows;
            var row = effectiveOrientation == System.Windows.Controls.Orientation.Horizontal ? i / columns : i % rows;

            columnWidths[col] = Math.Max(columnWidths[col], child.DesiredSize.Width);
            rowHeights[row] = Math.Max(rowHeights[row], child.DesiredSize.Height);
        }

        var totalWidth = columnWidths.Sum() + Spacing * (columns - 1);
        var totalHeight = rowHeights.Sum() + Spacing * (rows - 1);

        return new Size(totalWidth, totalHeight);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var itemCount = InternalChildren.Count;
        var effectiveOrientation = GetEffectiveOrientation();
        (var columns, var rows) = CalculateGridSize(itemCount, effectiveOrientation);

        var columnWidths = new double[columns];
        var rowHeights = new double[rows];

        for (var i = 0; i < itemCount; i++)
        {
            var child = InternalChildren[i];
            var col = effectiveOrientation == System.Windows.Controls.Orientation.Horizontal ? i % columns : i / rows;
            var row = effectiveOrientation == System.Windows.Controls.Orientation.Horizontal ? i / columns : i % rows;

            columnWidths[col] = Math.Max(columnWidths[col], child.DesiredSize.Width);
            rowHeights[row] = Math.Max(rowHeights[row], child.DesiredSize.Height);
        }

        if (StretchColumnIndex >= 0 && StretchColumnIndex < columns)
        {
            var usedWidth = columnWidths.Sum() + Spacing * (columns - 1);
            var extra = finalSize.Width - usedWidth;
            if (extra > 0)
                columnWidths[StretchColumnIndex] += extra;
        }

        if (StretchRowIndex >= 0 && StretchRowIndex < rows)
        {
            var usedHeight = rowHeights.Sum() + Spacing * (rows - 1);
            var extra = finalSize.Height - usedHeight;
            if (extra > 0)
                rowHeights[StretchRowIndex] += extra;
        }

        var y = 0.0;
        for (var row = 0; row < rows; row++)
        {
            var x = 0.0;
            for (var col = 0; col < columns; col++)
            {
                var index = effectiveOrientation == System.Windows.Controls.Orientation.Horizontal
                    ? row * columns + col
                    : col * rows + row;

                if (index >= itemCount)
                    break;

                var child = InternalChildren[index];
                child.Arrange(new Rect(x, y, columnWidths[col], rowHeights[row]));
                x += columnWidths[col] + Spacing;
            }
            y += rowHeights[row] + Spacing;
        }

        return finalSize;
    }

    /// <summary>
    /// Calculates the number of columns and rows needed to arrange the children based on the orientation.
    /// </summary>
    /// <param name="itemCount">Total number of child elements.</param>
    /// <param name="orientation">The layout orientation.</param>
    /// <returns>Tuple with number of columns and rows.</returns>
    private (int columns, int rows) CalculateGridSize(int itemCount, Orientation orientation)
    {
        int columns, rows;

        if (orientation == System.Windows.Controls.Orientation.Horizontal)
        {
            columns = Columns > 0 ? Columns : 1;
            rows = (int)Math.Ceiling((double)itemCount / columns);
        }
        else
        {
            rows = Rows > 0 ? Rows : 1;
            columns = (int)Math.Ceiling((double)itemCount / rows);
        }

        columns = Math.Max(1, columns);
        rows = Math.Max(1, rows);

        return (columns, rows);
    }

    /// <summary>
    /// Determines the effective orientation based on user settings and input properties.
    /// </summary>
    /// <returns>The orientation to be used for layout.</returns>
    private Orientation GetEffectiveOrientation()
    {
        if (Orientation.HasValue)
            return Orientation.Value;

        if (Columns > 0 && Rows == 0)
            return System.Windows.Controls.Orientation.Horizontal;

        if (Rows > 0 && Columns == 0)
            return System.Windows.Controls.Orientation.Vertical;

        return System.Windows.Controls.Orientation.Horizontal;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the number of columns. If set to 0, columns are auto-calculated based on rows and item count.
    /// </summary>
    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }
    public static readonly DependencyProperty ColumnsProperty
        = DependencyProperty.Register(
            nameof(Columns),
            typeof(int),
            typeof(StswDynamicGrid),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

    /// <summary>
    /// Gets or sets the layout orientation. If not set, it is auto-calculated based on Columns/Rows.
    /// </summary>
    public Orientation? Orientation
    {
        get => (Orientation?)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation?),
            typeof(StswDynamicGrid),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

    /// <summary>
    /// Gets or sets the number of rows. If set to 0, rows are auto-calculated based on columns and item count.
    /// </summary>
    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }
    public static readonly DependencyProperty RowsProperty
        = DependencyProperty.Register(
            nameof(Rows),
            typeof(int),
            typeof(StswDynamicGrid),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the spacing between elements.
    /// </summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
    public static readonly DependencyProperty SpacingProperty
        = DependencyProperty.Register(
            nameof(Spacing),
            typeof(double),
            typeof(StswDynamicGrid),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

    /// <summary>
    /// Gets or sets the index of the column that should stretch to fill extra available width.
    /// </summary>
    public int StretchColumnIndex
    {
        get => (int)GetValue(StretchColumnIndexProperty);
        set => SetValue(StretchColumnIndexProperty, value);
    }
    public static readonly DependencyProperty StretchColumnIndexProperty
        = DependencyProperty.Register(
            nameof(StretchColumnIndex),
            typeof(int),
            typeof(StswDynamicGrid),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsArrange)
        );

    /// <summary>
    /// Gets or sets the index of the row that should stretch to fill extra available height.
    /// </summary>
    public int StretchRowIndex
    {
        get => (int)GetValue(StretchRowIndexProperty);
        set => SetValue(StretchRowIndexProperty, value);
    }
    public static readonly DependencyProperty StretchRowIndexProperty
        = DependencyProperty.Register(
            nameof(StretchRowIndex),
            typeof(int),
            typeof(StswDynamicGrid),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsArrange)
        );
    #endregion
}
