using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Numerics;

namespace StswExpress;

/// <summary>
/// Represents a numeric column for <see cref="StswDataGrid"/> that allows entering and displaying numbers.
/// </summary>
[StswInfo("0.13.0")]
public abstract class StswDataGridNumberColumnBase<T, TControl> : DataGridTextColumn where T : struct, INumber<T> where TControl : StswNumberBoxBase<T>, new()
{
    static StswDataGridNumberColumnBase()
    {
        FontFamilyProperty.OverrideMetadata(typeof(StswDataGridNumberColumnBase<T, TControl>), new FrameworkPropertyMetadata(null));
        FontWeightProperty.OverrideMetadata(typeof(StswDataGridNumberColumnBase<T, TControl>), new FrameworkPropertyMetadata(null));
        FontSizeProperty.OverrideMetadata(typeof(StswDataGridNumberColumnBase<T, TControl>), new FrameworkPropertyMetadata(null));
        ForegroundProperty.OverrideMetadata(typeof(StswDataGridNumberColumnBase<T, TControl>), new FrameworkPropertyMetadata(null));
    }

    private static readonly Style StswEditingElementStyle = new(typeof(TControl), (Style)Application.Current.FindResource(typeof(TControl)))
    {
        Setters =
        {
            new Setter(StswNumberBoxBase<T>.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswNumberBoxBase<T>.CornerClippingProperty, false),
            new Setter(StswNumberBoxBase<T>.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswNumberBoxBase<T>.FocusVisualStyleProperty, null),
            new Setter(StswNumberBoxBase<T>.SeparatorThicknessProperty, 0.0),
            new Setter(StswNumberBoxBase<T>.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswNumberBoxBase<T>.VerticalAlignmentProperty, VerticalAlignment.Stretch)
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
        return GenerateEditingElement<TControl>();
    }

    /// <summary>
    /// Generates an editable numeric input element for entering values within the <see cref="DataGrid"/> column.
    /// Uses a generic numeric input control that extends <see cref="StswNumberBoxBase{T}"/>.
    /// </summary>
    /// <param name="cell">The <see cref="DataGridCell"/> that will contain the element.</param>
    /// <param name="dataItem">The data item represented by the row containing the cell.</param>
    /// <returns>An input control of type <typeparamref name="TControl"/> bound to the column's numeric value.</returns>
#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
    private TControl GenerateEditingElement<TControl>() where TControl : StswNumberBoxBase<T>, new()
#pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type
    {
        var editingElement = new TControl
        {
            Style = StswEditingElementStyle,
            Format = Format,
            Padding = Padding,
            Placeholder = Placeholder,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswNumberBoxBase<T>.ValueProperty, Binding);

        return editingElement;
    }


    #region Logic properties
    /// <summary>
    /// Gets or sets the numeric format used for displaying values (e.g., "N2" for two decimal places, "C2" for currency).
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
        );

    /// <summary>
    /// Gets or sets the placeholder text displayed in the numeric input when no value is entered.
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the padding around the numeric input inside the column's cells.
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
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
            typeof(StswDataGridNumberColumnBase<T, TControl>)
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
            typeof(StswDataGridNumberColumnBase<T, TControl>),
            new PropertyMetadata(TextWrapping.NoWrap)
        );

    /// <summary>
    /// Gets or sets the horizontal alignment of the numeric input inside the editing element.
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
            typeof(StswDataGridNumberColumnBase<T, TControl>),
            new PropertyMetadata(HorizontalAlignment.Left)
        );

    /// <summary>
    /// Gets or sets the vertical alignment of the numeric input inside the editing element.
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
            typeof(StswDataGridNumberColumnBase<T, TControl>),
            new PropertyMetadata(VerticalAlignment.Top)
        );
    #endregion
}

/// <summary>
/// Represents a numeric column for decimal values within <see cref="StswDataGrid"/>.
/// Uses <see cref="StswDecimalBox"/> as the editing control.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridDecimalColumn Header="Price" Binding="{Binding Price}" Format="C2"/&gt;
/// </code>
/// </example>
[StswInfo("0.13.0")]
public class StswDataGridDecimalColumn : StswDataGridNumberColumnBase<decimal, StswDecimalBox> { }

/// <summary>
/// Represents a numeric column for double-precision floating-point values within <see cref="StswDataGrid"/>.
/// Uses <see cref="StswDoubleBox"/> as the editing control.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridDoubleColumn Header="Price" Binding="{Binding Price}" Format="C2"/&gt;
/// </code>
/// </example>
[StswInfo("0.13.0")]
public class StswDataGridDoubleColumn : StswDataGridNumberColumnBase<double, StswDoubleBox> { }

/// <summary>
/// Represents a numeric column for integer values within <see cref="StswDataGrid"/>.
/// Uses <see cref="StswIntegerBox"/> as the editing control.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridIntegerColumn Header="Price" Binding="{Binding Price}" Format="C2"/&gt;
/// </code>
/// </example>
[StswInfo("0.13.0")]
public class StswDataGridIntegerColumn : StswDataGridNumberColumnBase<int, StswIntegerBox> { }
