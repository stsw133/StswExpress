using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a text column for <see cref="StswDataGrid"/> that allows displaying and editing text.
/// </summary>
public class StswDataGridTextColumn : DataGridTextColumn
{
    //private static readonly Style StswDisplayElementStyle = new(typeof(StswText), (Style)Application.Current.FindResource(typeof(StswText)));
    private static readonly Style StswEditingElementStyle = new(typeof(StswTextBox), (Style)Application.Current.FindResource(typeof(StswTextBox)))
    {
        Setters =
        {
            new Setter(StswTextBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswTextBox.CornerClippingProperty, false),
            new Setter(StswTextBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswTextBox.FocusVisualStyleProperty, null),
            new Setter(StswTextBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswTextBox.VerticalAlignmentProperty, VerticalAlignment.Stretch),
        }
    };

    /// <summary>
    /// Generates a non-editable text element for display within the <see cref="DataGrid"/> column.
    /// Uses <see cref="StswText"/> as the display element.
    /// </summary>
    /// <param name="cell">The <see cref="DataGridCell"/> that will contain the element.</param>
    /// <param name="dataItem">The data item represented by the row containing the cell.</param>
    /// <returns>A <see cref="StswText"/> element bound to the column's data.</returns>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswText()
        {
            //Style = StswDisplayElementStyle,
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
    /// Generates an editable text element for inline editing within the <see cref="DataGrid"/> column.
    /// Uses <see cref="StswTextBox"/> as the editing element.
    /// </summary>
    /// <param name="cell">The <see cref="DataGridCell"/> that will contain the element.</param>
    /// <param name="dataItem">The data item represented by the row containing the cell.</param>
    /// <returns>A <see cref="StswTextBox"/> element bound to the column's data.</returns>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswTextBox()
        {
            Style = StswEditingElementStyle,
            AcceptsReturn = AcceptsReturn,
            MaxLength = MaxLength,
            Padding = Padding,
            Placeholder = Placeholder,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswTextBox.TextProperty, Binding);

        return editingElement;
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="StswTextBox"/> 
    /// in the column supports multi-line text input by accepting the Enter key.
    /// </summary>
    public bool AcceptsReturn
    {
        get => (bool)GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }
    public static readonly DependencyProperty AcceptsReturnProperty
        = DependencyProperty.Register(
            nameof(AcceptsReturnProperty),
            typeof(bool),
            typeof(StswDataGridTextColumn)
        );

    /// <summary>
    /// Gets or sets the maximum number of characters allowed in the editable text box.
    /// </summary>
    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }
    public static readonly DependencyProperty MaxLengthProperty
        = DependencyProperty.Register(
            nameof(MaxLengthProperty),
            typeof(int),
            typeof(StswDataGridTextColumn)
        );

    /// <summary>
    /// Gets or sets the placeholder text displayed in the editing element when it is empty.
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
            typeof(StswDataGridTextColumn)
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
            typeof(StswDataGridTextColumn)
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
            typeof(StswDataGridTextColumn)
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
            typeof(StswDataGridTextColumn)
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
            typeof(StswDataGridTextColumn),
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
            typeof(StswDataGridTextColumn),
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
            typeof(StswDataGridTextColumn),
            new PropertyMetadata(VerticalAlignment.Top)
        );
    #endregion
}

/* usage:

<se:StswDataGridTextColumn Header="Name" Binding="{Binding Name}" Placeholder="Enter name"/>

*/
