using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a file path column for <see cref="StswDataGrid"/> that allows selecting file or folder paths.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridPathColumn Header="File Path" Binding="{Binding FilePath}" SelectionUnit="OpenFile"/&gt;
/// </code>
/// </example>
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
            new Setter(StswPathPicker.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswPathPicker.VerticalAlignmentProperty, VerticalAlignment.Stretch)
        }
    };

    /// <inheritdoc/>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswText()
        {
            Margin = new Thickness(2, 0, 2, 0)
        };
        displayElement.SetBinding(TextBlock.PaddingProperty, CreateColumnBinding(nameof(Padding)));
        displayElement.SetBinding(TextBlock.TextAlignmentProperty, CreateColumnBinding(nameof(TextAlignment)));
        displayElement.SetBinding(TextBlock.TextTrimmingProperty, CreateColumnBinding(nameof(TextTrimming)));
        displayElement.SetBinding(TextBlock.TextWrappingProperty, CreateColumnBinding(nameof(TextWrapping)));
        StswDataGridTextColumn.BindFontProperties(this, displayElement);

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(displayElement, TextBlock.TextProperty, Binding);

        return displayElement;
    }

    /// <inheritdoc/>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswPathPicker()
        {
            Style = StswEditingElementStyle
        };
        editingElement.SetBinding(StswPathPicker.FilterProperty, CreateColumnBinding(nameof(Filter)));
        editingElement.SetBinding(StswPathPicker.IsFileSizeVisibleProperty, CreateColumnBinding(nameof(IsFileSizeVisible)));
        editingElement.SetBinding(StswPathPicker.IsShiftingEnabledProperty, CreateColumnBinding(nameof(IsShiftingEnabled)));
        editingElement.SetBinding(StswPathPicker.MultiselectProperty, CreateColumnBinding(nameof(Multiselect)));
        editingElement.SetBinding(StswPathPicker.PaddingProperty, CreateColumnBinding(nameof(Padding)));
        editingElement.SetBinding(StswPathPicker.PlaceholderProperty, CreateColumnBinding(nameof(Placeholder)));
        editingElement.SetBinding(StswPathPicker.SelectionUnitProperty, CreateColumnBinding(nameof(SelectionUnit)));
        editingElement.SetBinding(StswPathPicker.SuggestedFilenameProperty, CreateColumnBinding(nameof(SuggestedFilename)));
        editingElement.SetBinding(StswPathPicker.HorizontalContentAlignmentProperty, CreateColumnBinding(nameof(HorizontalContentAlignment)));
        editingElement.SetBinding(StswPathPicker.VerticalContentAlignmentProperty, CreateColumnBinding(nameof(VerticalContentAlignment)));

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswPathPicker.SelectedPathProperty, Binding);

        return editingElement;
    }

    /// <summary>
    /// Creates a one-way binding to a property of this column.
    /// </summary>
    /// <param name="propertyName">The name of the property to bind to.</param>
    /// <returns>A one-way binding to the specified property.</returns>
    private Binding CreateColumnBinding(string propertyName) => new(propertyName)
    {
        Source = this,
        Mode = BindingMode.OneWay
    };

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
    /// Gets or sets a value indicating whether shifting through adjacent paths is enabled.
    /// If enabled, users can navigate between directories or files in the current folder using mouse wheel or keyboard keys.
    /// </summary>
    public bool IsShiftingEnabled
    {
        get => (bool)GetValue(IsShiftingEnabledProperty);
        set => SetValue(IsShiftingEnabledProperty, value);
    }
    public static readonly DependencyProperty IsShiftingEnabledProperty
        = DependencyProperty.Register(
            nameof(IsShiftingEnabled),
            typeof(bool),
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
            nameof(Placeholder),
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

    /// <summary>
    /// Gets or sets the suggested file name for file dialog default file name.
    /// Provides a default name for files when the save dialog is shown.
    /// </summary>
    public string? SuggestedFilename
    {
        get => (string?)GetValue(SuggestedFilenameProperty);
        set => SetValue(SuggestedFilenameProperty, value);
    }
    public static readonly DependencyProperty SuggestedFilenameProperty
        = DependencyProperty.Register(
            nameof(SuggestedFilename),
            typeof(string),
            typeof(StswDataGridPathColumn)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets whether to show or not the file size.
    /// If true, the size of the selected file is displayed next to the selected path.
    /// </summary>
    public bool IsFileSizeVisible
    {
        get => (bool)GetValue(IsFileSizeVisibleProperty);
        set => SetValue(IsFileSizeVisibleProperty, value);
    }
    public static readonly DependencyProperty IsFileSizeVisibleProperty
        = DependencyProperty.Register(
            nameof(IsFileSizeVisible),
            typeof(bool),
            typeof(StswDataGridPathColumn)
        );

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
            nameof(TextAlignment),
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
            nameof(TextTrimming),
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
            nameof(TextWrapping),
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
