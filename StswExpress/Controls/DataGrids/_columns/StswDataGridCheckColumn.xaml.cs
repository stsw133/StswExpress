using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a checkbox column for <see cref="StswDataGrid"/> that allows selecting boolean values.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridCheckColumn Header="Active" Binding="{Binding IsActive}"/&gt;
/// </code>
/// </example>
public class StswDataGridCheckColumn : DataGridCheckBoxColumn
{
    private static readonly Style StswDisplayElementStyle = new(typeof(StswCheckBox), (Style)Application.Current.FindResource(typeof(StswCheckBox)))
    {
        Setters =
        {
            new Setter(StswCheckBox.BorderThicknessProperty, new Thickness(1)),
            new Setter(StswCheckBox.CornerClippingProperty, false),
            new Setter(StswCheckBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswCheckBox.FocusableProperty, false),
            new Setter(StswCheckBox.FocusVisualStyleProperty, null),
            new Setter(StswCheckBox.IsHitTestVisibleProperty, false),
            new Setter(StswCheckBox.IsTabStopProperty, false)
        }
    };
    private static readonly Style StswEditingElementStyle = new(typeof(StswCheckBox), (Style)Application.Current.FindResource(typeof(StswCheckBox)))
    {
        Setters =
        {
            new Setter(StswCheckBox.BorderThicknessProperty, new Thickness(1)),
            new Setter(StswCheckBox.CornerClippingProperty, false),
            new Setter(StswCheckBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswCheckBox.FocusVisualStyleProperty, null)
        }
    };

    /// <inheritdoc/>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswCheckBox()
        {
            Style = StswDisplayElementStyle
        };
        displayElement.SetBinding(StswCheckBox.PaddingProperty, CreateColumnBinding(nameof(Padding)));
        displayElement.SetBinding(StswCheckBox.HorizontalAlignmentProperty, CreateColumnBinding(nameof(HorizontalContentAlignment)));
        displayElement.SetBinding(StswCheckBox.HorizontalContentAlignmentProperty, CreateColumnBinding(nameof(HorizontalContentAlignment)));
        displayElement.SetBinding(StswCheckBox.VerticalAlignmentProperty, CreateColumnBinding(nameof(VerticalContentAlignment)));
        displayElement.SetBinding(StswCheckBox.VerticalContentAlignmentProperty, CreateColumnBinding(nameof(VerticalContentAlignment)));
        ApplyIconBindings(displayElement);

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(displayElement, StswCheckBox.IsCheckedProperty, Binding);

        return displayElement;
    }

    /// <inheritdoc/>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswCheckBox()
        {
            Style = StswEditingElementStyle
        };
        editingElement.SetBinding(StswCheckBox.PaddingProperty, CreateColumnBinding(nameof(Padding)));
        editingElement.SetBinding(StswCheckBox.HorizontalAlignmentProperty, CreateColumnBinding(nameof(HorizontalContentAlignment)));
        editingElement.SetBinding(StswCheckBox.HorizontalContentAlignmentProperty, CreateColumnBinding(nameof(HorizontalContentAlignment)));
        editingElement.SetBinding(StswCheckBox.VerticalAlignmentProperty, CreateColumnBinding(nameof(VerticalContentAlignment)));
        editingElement.SetBinding(StswCheckBox.VerticalContentAlignmentProperty, CreateColumnBinding(nameof(VerticalContentAlignment)));
        ApplyIconBindings(editingElement);

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswCheckBox.IsCheckedProperty, Binding);

        return editingElement;
    }

    /// <summary>
    /// Applies the icon-related bindings from this column to the specified checkbox element.
    /// </summary>
    /// <param name="element">The checkbox element to apply the bindings to.</param>
    private void ApplyIconBindings(StswCheckBox element)
    {
        element.SetBinding(StswCheckBox.IconCheckedProperty, CreateColumnBinding(nameof(IconChecked)));
        element.SetBinding(StswCheckBox.IconUncheckedProperty, CreateColumnBinding(nameof(IconUnchecked)));
        element.SetBinding(StswCheckBox.IconIndeterminateProperty, CreateColumnBinding(nameof(IconIndeterminate)));

        var iconScaleBinding = CreateColumnBinding(nameof(IconScale));
        iconScaleBinding.TargetNullValue = element.GetValue(StswCheckBox.IconScaleProperty);
        iconScaleBinding.FallbackValue = element.GetValue(StswCheckBox.IconScaleProperty);
        element.SetBinding(StswCheckBox.IconScaleProperty, iconScaleBinding);
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
    /// Gets or sets the scale of the icon inside the checkbox.
    /// </summary>
    public GridLength? IconScale
    {
        get => (GridLength?)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength?),
            typeof(StswDataGridCheckColumn)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the geometry used for the icon when the checkbox is in the checked state.
    /// </summary>
    public Geometry? IconChecked
    {
        get => (Geometry?)GetValue(IconCheckedProperty);
        set => SetValue(IconCheckedProperty, value);
    }
    public static readonly DependencyProperty IconCheckedProperty
        = DependencyProperty.Register(
            nameof(IconChecked),
            typeof(Geometry),
            typeof(StswDataGridCheckColumn),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon when the checkbox is in the indeterminate state.
    /// </summary>
    public Geometry? IconIndeterminate
    {
        get => (Geometry?)GetValue(IconIndeterminateProperty);
        set => SetValue(IconIndeterminateProperty, value);
    }
    public static readonly DependencyProperty IconIndeterminateProperty
        = DependencyProperty.Register(
            nameof(IconIndeterminate),
            typeof(Geometry),
            typeof(StswDataGridCheckColumn),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon when the checkbox is in the unchecked state.
    /// </summary>
    public Geometry? IconUnchecked
    {
        get => (Geometry?)GetValue(IconUncheckedProperty);
        set => SetValue(IconUncheckedProperty, value);
    }
    public static readonly DependencyProperty IconUncheckedProperty
        = DependencyProperty.Register(
            nameof(IconUnchecked),
            typeof(Geometry),
            typeof(StswDataGridCheckColumn),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the padding around the checkbox inside the column's cells.
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
            typeof(StswDataGridCheckColumn)
        );

    /// <summary>
    /// Gets or sets the horizontal alignment of the checkbox inside the column's cells.
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
            typeof(StswDataGridCheckColumn),
            new PropertyMetadata(HorizontalAlignment.Center)
        );

    /// <summary>
    /// Gets or sets the vertical alignment of the checkbox inside the column's cells.
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
            typeof(StswDataGridCheckColumn),
            new PropertyMetadata(VerticalAlignment.Center)
        );
    #endregion
}
