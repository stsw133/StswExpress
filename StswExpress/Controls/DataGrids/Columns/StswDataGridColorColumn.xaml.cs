using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a color column for <see cref="StswDataGrid"/> that allows selecting and displaying colors.
/// </summary>
public class StswDataGridColorColumn : DataGridTextColumn
{
    //private static readonly Style StswDisplayElementStyle = new(typeof(StswText), (Style)Application.Current.FindResource(typeof(StswText)));
    private static readonly Style StswEditingElementStyle = new(typeof(StswColorBox), (Style)Application.Current.FindResource(typeof(StswColorBox)))
    {
        Setters =
        {
            new Setter(StswColorBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswColorBox.CornerClippingProperty, false),
            new Setter(StswColorBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswColorBox.FocusVisualStyleProperty, null),
            new Setter(StswColorBox.SeparatorThicknessProperty, 0.0),
            new Setter(StswColorBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswColorBox.VerticalAlignmentProperty, VerticalAlignment.Stretch)
        }
    };

    /// <summary>
    /// Generates a non-editable text element for displaying color values within the <see cref="DataGrid"/> column.
    /// Uses <see cref="StswText"/> as the display element.
    /// </summary>
    /// <param name="cell">The <see cref="DataGridCell"/> that will contain the element.</param>
    /// <param name="dataItem">The data item represented by the row containing the cell.</param>
    /// <returns>A <see cref="StswText"/> element bound to the column's color data.</returns>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswText()
        {
            //Style = StswDisplayElementStyle,
            FontWeight = FontWeight,
            Margin = new Thickness(2, 0, 2, 0),
            Padding = Padding,
            TextAlignment = TextAlignment,
            TextTrimming = TextTrimming,
            TextWrapping = TextWrapping
        };

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, Binding);

        return displayElement;
    }

    /// <summary>
    /// Generates an editable color picker element for selecting color values within the <see cref="DataGrid"/> column.
    /// Uses <see cref="StswColorBox"/> as the editing element.
    /// </summary>
    /// <param name="cell">The <see cref="DataGridCell"/> that will contain the element.</param>
    /// <param name="dataItem">The data item represented by the row containing the cell.</param>
    /// <returns>A <see cref="StswColorBox"/> element bound to the column's color data.</returns>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswColorBox()
        {
            Style = StswEditingElementStyle,
            Padding = Padding,
            Placeholder = Placeholder,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswColorBox.SelectedColorProperty, Binding);

        return editingElement;
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the placeholder text displayed in the editing element when no color is selected.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(PlaceholderProperty),
            typeof(string),
            typeof(StswDataGridColorColumn)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the padding around the color box inside the column's cells.
    /// </summary>
    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }
    public static readonly DependencyProperty PaddingProperty
        = DependencyProperty.Register(
            nameof(PaddingProperty),
            typeof(Thickness),
            typeof(StswDataGridColorColumn)
        );

    /// <summary>
    /// Gets or sets the horizontal text alignment for both display and editing elements in the column.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
    public static readonly DependencyProperty TextAlignmentProperty
        = DependencyProperty.Register(
            nameof(TextAlignmentProperty),
            typeof(TextAlignment),
            typeof(StswDataGridColorColumn)
        );

    /// <summary>
    /// Gets or sets how the text is trimmed when it overflows the available width in the display element.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    public static readonly DependencyProperty TextTrimmingProperty
        = DependencyProperty.Register(
            nameof(TextTrimmingProperty),
            typeof(TextTrimming),
            typeof(StswDataGridColorColumn)
        );

    /// <summary>
    /// Gets or sets whether the text wraps within the column's cells when it exceeds the available space.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    public static readonly DependencyProperty TextWrappingProperty
        = DependencyProperty.Register(
            nameof(TextWrappingProperty),
            typeof(TextWrapping),
            typeof(StswDataGridColorColumn),
            new PropertyMetadata(TextWrapping.NoWrap)
        );

    /// <summary>
    /// Gets or sets the horizontal alignment of the color box inside the editing element.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }
    public static readonly DependencyProperty HorizontalContentAlignmentProperty
        = DependencyProperty.Register(
            nameof(HorizontalContentAlignment),
            typeof(HorizontalAlignment),
            typeof(StswDataGridColorColumn),
            new PropertyMetadata(HorizontalAlignment.Left)
        );

    /// <summary>
    /// Gets or sets the vertical alignment of the color box inside the editing element.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }
    public static readonly DependencyProperty VerticalContentAlignmentProperty
        = DependencyProperty.Register(
            nameof(VerticalContentAlignment),
            typeof(VerticalAlignment),
            typeof(StswDataGridColorColumn),
            new PropertyMetadata(VerticalAlignment.Top)
        );
    #endregion
}

/* usage:

<se:StswDataGridColorColumn Header="Theme Color" Binding="{Binding ThemeColor}" Placeholder="Select color"/>

*/
