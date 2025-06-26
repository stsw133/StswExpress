using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a file path column for <see cref="StswDataGrid"/> that allows selecting file or folder paths.
/// </summary>
public class StswDataGridPathColumn : DataGridTextColumn
{
    static StswDataGridPathColumn()
    {
        FontFamilyProperty.OverrideMetadata(typeof(StswDataGridPathColumn), new FrameworkPropertyMetadata(null));
        FontWeightProperty.OverrideMetadata(typeof(StswDataGridPathColumn), new FrameworkPropertyMetadata(null));
        FontSizeProperty.OverrideMetadata(typeof(StswDataGridPathColumn), new FrameworkPropertyMetadata(null));
        ForegroundProperty.OverrideMetadata(typeof(StswDataGridPathColumn), new FrameworkPropertyMetadata(null));
    }

    private static readonly Style StswEditingElementStyle = new(typeof(StswPathPicker), (Style)Application.Current.FindResource(typeof(StswPathPicker)))
    {
        Setters =
        {
            new Setter(StswPathPicker.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswPathPicker.CornerClippingProperty, false),
            new Setter(StswPathPicker.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswPathPicker.FocusVisualStyleProperty, null),
            new Setter(StswPathPicker.SeparatorThicknessProperty, 0.0),
            new Setter(StswPathPicker.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswPathPicker.VerticalAlignmentProperty, VerticalAlignment.Stretch)
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
        var editingElement = new StswPathPicker()
        {
            Style = StswEditingElementStyle,
            Filter = Filter,
            Multiselect = Multiselect,
            Padding = Padding,
            Placeholder = Placeholder,
            SelectionUnit = SelectionUnit,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment,
        };

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswPathPicker.SelectedPathProperty, Binding);

        return editingElement;
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the file filter used in the file selection dialog.
    /// Example: "Image Files (*.png;*.jpg)|*.png;*.jpg".
    /// </summary>
    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    public static readonly DependencyProperty FilterProperty
        = DependencyProperty.Register(
            nameof(Filter),
            typeof(string),
            typeof(StswDataGridPathColumn)
        );

    /// <summary>
    /// Gets or sets a value indicating whether multiple file or folder selections are allowed.
    /// </summary>
    public bool Multiselect
    {
        get => (bool)GetValue(MultiselectProperty);
        set => SetValue(MultiselectProperty, value);
    }
    public static readonly DependencyProperty MultiselectProperty
        = DependencyProperty.Register(
            nameof(Multiselect),
            typeof(bool),
            typeof(StswDataGridPathColumn)
        );

    /// <summary>
    /// Gets or sets the placeholder text displayed in the path picker when no value is selected.
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
            typeof(StswDataGridPathColumn)
        );

    /// <summary>
    /// Gets or sets the selection unit for the path picker.
    /// Determines whether users can select files, folders, or save locations.
    /// </summary>
    public StswPathType SelectionUnit
    {
        get => (StswPathType)GetValue(SelectionUnitProperty);
        set => SetValue(SelectionUnitProperty, value);
    }
    public static readonly DependencyProperty SelectionUnitProperty
        = DependencyProperty.Register(
            nameof(SelectionUnit),
            typeof(StswPathType),
            typeof(StswDataGridPathColumn),
            new PropertyMetadata(StswPathType.OpenFile)
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
            nameof(PaddingProperty),
            typeof(Thickness),
            typeof(StswDataGridPathColumn)
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
            typeof(StswDataGridPathColumn)
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
            typeof(StswDataGridPathColumn)
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
            typeof(StswDataGridPathColumn),
            new PropertyMetadata(TextWrapping.NoWrap)
        );

    /// <summary>
    /// Gets or sets the horizontal alignment of the path picker inside the editing element.
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
            typeof(StswDataGridPathColumn),
            new PropertyMetadata(HorizontalAlignment.Left)
        );

    /// <summary>
    /// Gets or sets the vertical alignment of the path picker inside the editing element.
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
            typeof(StswDataGridPathColumn),
            new PropertyMetadata(VerticalAlignment.Top)
        );
    #endregion
}

/* usage:

<se:StswDataGridPathColumn Header="File Path" Binding="{Binding FilePath}" SelectionUnit="OpenFile"/>

*/
