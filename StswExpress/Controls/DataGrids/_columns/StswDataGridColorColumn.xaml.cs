using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a color column for <see cref="StswDataGrid"/> that allows selecting and displaying colors.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridColorColumn Header="Theme Color" Binding="{Binding ThemeColor}" Placeholder="Select color"/&gt;
/// </code>
/// </example>
[StswInfo("0.13.0", IsTested = false)]
public class StswDataGridColorColumn : DataGridTextColumn
{
    static StswDataGridColorColumn()
    {
        FontFamilyProperty.OverrideMetadata(typeof(StswDataGridColorColumn), new FrameworkPropertyMetadata(null));
        FontWeightProperty.OverrideMetadata(typeof(StswDataGridColorColumn), new FrameworkPropertyMetadata(null));
        FontSizeProperty.OverrideMetadata(typeof(StswDataGridColorColumn), new FrameworkPropertyMetadata(null));
        ForegroundProperty.OverrideMetadata(typeof(StswDataGridColorColumn), new FrameworkPropertyMetadata(null));
    }

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
        var editingElement = new StswColorBox()
        {
            Style = StswEditingElementStyle,
            IsAlphaEnabled = IsAlphaEnabled,
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
    /// Gets or sets a value indicating whether the alpha channel (transparency) is enabled for color selection.
    /// When disabled, the selected color will always have full opacity.
    /// </summary>
    [StswInfo("0.20.0")]
    public bool IsAlphaEnabled
    {
        get => (bool)GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }
    public static readonly DependencyProperty IsAlphaEnabledProperty
        = DependencyProperty.Register(
            nameof(IsAlphaEnabled),
            typeof(bool),
            typeof(StswDataGridColorColumn)
        );

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
            nameof(Placeholder),
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
            nameof(Padding),
            typeof(Thickness),
            typeof(StswDataGridColorColumn)
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
            nameof(TextAlignment),
            typeof(TextAlignment),
            typeof(StswDataGridColorColumn)
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
            nameof(TextTrimming),
            typeof(TextTrimming),
            typeof(StswDataGridColorColumn)
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
            nameof(TextWrapping),
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
