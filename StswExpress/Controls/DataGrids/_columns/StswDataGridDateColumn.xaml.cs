using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a date column for <see cref="StswDataGrid"/> that allows selecting and displaying dates.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridDateColumn Header="Birth Date" Binding="{Binding BirthDate}" Format="dd/MM/yyyy"/&gt;
/// </code>
/// </example>
[StswInfo("0.13.0")]
public class StswDataGridDateColumn : DataGridTextColumn
{
    static StswDataGridDateColumn()
    {
        FontFamilyProperty.OverrideMetadata(typeof(StswDataGridDateColumn), new FrameworkPropertyMetadata(null));
        FontWeightProperty.OverrideMetadata(typeof(StswDataGridDateColumn), new FrameworkPropertyMetadata(null));
        FontSizeProperty.OverrideMetadata(typeof(StswDataGridDateColumn), new FrameworkPropertyMetadata(null));
        ForegroundProperty.OverrideMetadata(typeof(StswDataGridDateColumn), new FrameworkPropertyMetadata(null));
    }

    private static readonly Style StswEditingElementStyle = new(typeof(StswDatePicker), (Style)Application.Current.FindResource(typeof(StswDatePicker)))
    {
        Setters =
        {
            new Setter(StswDatePicker.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswDatePicker.CornerClippingProperty, false),
            new Setter(StswDatePicker.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswDatePicker.FocusVisualStyleProperty, null),
            new Setter(StswDatePicker.PaddingProperty, new Thickness(0)),
            new Setter(StswDatePicker.SeparatorThicknessProperty, 0d),
            new Setter(StswDatePicker.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswDatePicker.VerticalAlignmentProperty, VerticalAlignment.Stretch)
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
        var editingElement = new StswDatePicker()
        {
            Style = StswEditingElementStyle,
            Format = Format,
            Padding = Padding,
            Placeholder = Placeholder,
            SelectionUnit = SelectionUnit,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswDatePicker.SelectedDateProperty, Binding);

        return editingElement;
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the date format displayed in the column.
    /// Example: "dd/MM/yyyy".
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
            typeof(StswDataGridDateColumn)
        );

    /// <summary>
    /// Gets or sets the placeholder text displayed in the date picker when no value is selected.
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
            typeof(StswDataGridDateColumn)
        );

    /// <summary>
    /// Gets or sets the selection unit for the date picker.
    /// Determines whether users can select days, months, or years.
    /// </summary>
    public StswCalendarUnit SelectionUnit
    {
        get => (StswCalendarUnit)GetValue(SelectionUnitProperty);
        set => SetValue(SelectionUnitProperty, value);
    }
    public static readonly DependencyProperty SelectionUnitProperty
        = DependencyProperty.Register(
            nameof(SelectionUnit),
            typeof(StswCalendarUnit),
            typeof(StswDataGridDateColumn),
            new PropertyMetadata(StswCalendarUnit.Days)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the padding around the content inside the column's cells.
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
            typeof(StswDataGridDateColumn)
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
            typeof(StswDataGridDateColumn)
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
            typeof(StswDataGridDateColumn)
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
            typeof(StswDataGridDateColumn),
            new PropertyMetadata(TextWrapping.NoWrap)
        );

    /// <summary>
    /// Gets or sets the horizontal alignment of the date picker inside the editing element.
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
            typeof(StswDataGridDateColumn),
            new PropertyMetadata(HorizontalAlignment.Left)
        );

    /// <summary>
    /// Gets or sets the vertical alignment of the date picker inside the editing element.
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
            typeof(StswDataGridDateColumn),
            new PropertyMetadata(VerticalAlignment.Top)
        );
    #endregion
}
