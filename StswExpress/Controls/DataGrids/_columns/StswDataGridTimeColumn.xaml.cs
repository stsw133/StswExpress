using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a time column for <see cref="StswDataGrid"/> that allows selecting and displaying time values.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridTimeColumn Header="Time" Binding="{Binding StartTime}" Format="HH:mm" Placeholder="Select time"/&gt;
/// </code>
/// </example>
[StswInfo("0.16.0")]
public class StswDataGridTimeColumn : DataGridTextColumn
{
    static StswDataGridTimeColumn()
    {
        FontFamilyProperty.OverrideMetadata(typeof(StswDataGridTimeColumn), new FrameworkPropertyMetadata(null));
        FontWeightProperty.OverrideMetadata(typeof(StswDataGridTimeColumn), new FrameworkPropertyMetadata(null));
        FontSizeProperty.OverrideMetadata(typeof(StswDataGridTimeColumn), new FrameworkPropertyMetadata(null));
        ForegroundProperty.OverrideMetadata(typeof(StswDataGridTimeColumn), new FrameworkPropertyMetadata(null));
    }

    private static readonly Style StswEditingElementStyle = new(typeof(StswTimePicker), (Style)Application.Current.FindResource(typeof(StswTimePicker)))
    {
        Setters =
        {
            new Setter(StswTimePicker.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswTimePicker.CornerClippingProperty, false),
            new Setter(StswTimePicker.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswTimePicker.FocusVisualStyleProperty, null),
            new Setter(StswTimePicker.PaddingProperty, new Thickness(0)),
            new Setter(StswTimePicker.SeparatorThicknessProperty, 0d),
            new Setter(StswTimePicker.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswTimePicker.VerticalAlignmentProperty, VerticalAlignment.Stretch),
        }
    };

    /// <inheritdoc/>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswText()
        {
            Margin = new Thickness(2, 0, 2, 0),
            Padding = Padding,
            TextAlignment = TextAlignment,
            TextTrimming = TextTrimming,
            TextWrapping = TextWrapping
        };
        StswDataGridTextColumn.BindFontProperties(this, displayElement);

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, Binding);

        return displayElement;
    }

    /// <inheritdoc/>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswTimePicker()
        {
            Style = StswEditingElementStyle,
            Format = Format,
            Padding = Padding,
            Placeholder = Placeholder,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswTimePicker.SelectedTimeProperty, Binding);

        return editingElement;
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the format used for displaying the time value.
    /// The format follows standard time formatting conventions, such as "HH:mm".
    /// </summary>
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static readonly DependencyProperty FormatProperty
        = DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(StswDataGridTimeColumn)
        );

    /// <summary>
    /// Gets or sets the placeholder text displayed in the editing element when no time value is selected.
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
            typeof(StswDataGridTimeColumn)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the padding around the text inside the column's cells.
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
            typeof(StswDataGridTimeColumn)
        );

    /// <summary>
    /// Gets or sets the horizontal text alignment for both display and editing elements in the column.
    /// </summary>
    [StswInfo("0.16.0")]
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
    public static readonly DependencyProperty TextAlignmentProperty
        = DependencyProperty.Register(
            nameof(TextAlignmentProperty),
            typeof(TextAlignment),
            typeof(StswDataGridTimeColumn)
        );

    /// <summary>
    /// Gets or sets how the text is trimmed when it overflows the available width in the display element.
    /// </summary>
    [StswInfo("0.16.0")]
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    public static readonly DependencyProperty TextTrimmingProperty
        = DependencyProperty.Register(
            nameof(TextTrimmingProperty),
            typeof(TextTrimming),
            typeof(StswDataGridTimeColumn)
        );

    /// <summary>
    /// Gets or sets whether the text wraps within the column's cells when it exceeds the available space.
    /// </summary>
    [StswInfo("0.16.1")]
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    public static readonly DependencyProperty TextWrappingProperty
        = DependencyProperty.Register(
            nameof(TextWrappingProperty),
            typeof(TextWrapping),
            typeof(StswDataGridTimeColumn),
            new PropertyMetadata(TextWrapping.NoWrap)
        );

    /// <summary>
    /// Gets or sets the horizontal alignment of the text content inside the editing element.
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
            typeof(StswDataGridTimeColumn),
            new PropertyMetadata(HorizontalAlignment.Left)
        );

    /// <summary>
    /// Gets or sets the vertical alignment of the text content inside the editing element.
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
            typeof(StswDataGridTimeColumn),
            new PropertyMetadata(VerticalAlignment.Top)
        );
    #endregion
}
