﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a checkbox column for <see cref="StswDataGrid"/> that allows selecting boolean values.
/// </summary>
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

    /// <summary>
    /// Generates a non-editable checkbox element for displaying boolean values within the <see cref="DataGrid"/> column.
    /// Uses <see cref="StswCheckBox"/> as the display element.
    /// </summary>
    /// <param name="cell">The <see cref="DataGridCell"/> that will contain the element.</param>
    /// <param name="dataItem">The data item represented by the row containing the cell.</param>
    /// <returns>A <see cref="StswCheckBox"/> element bound to the column's boolean data.</returns>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswCheckBox()
        {
            Style = StswDisplayElementStyle,
            Padding = Padding,
            HorizontalAlignment = HorizontalContentAlignment,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalAlignment = VerticalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(displayElement, StswCheckBox.IsCheckedProperty, Binding);

        return displayElement;
    }

    /// <summary>
    /// Generates an editable checkbox element for inline editing within the <see cref="DataGrid"/> column.
    /// Uses <see cref="StswCheckBox"/> as the editing element.
    /// </summary>
    /// <param name="cell">The <see cref="DataGridCell"/> that will contain the element.</param>
    /// <param name="dataItem">The data item represented by the row containing the cell.</param>
    /// <returns>A <see cref="StswCheckBox"/> element bound to the column's boolean data.</returns>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswCheckBox()
        {
            Style = StswEditingElementStyle,
            Padding = Padding,
            HorizontalAlignment = HorizontalContentAlignment,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalAlignment = VerticalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswCheckBox.IsCheckedProperty, Binding);

        return editingElement;
    }

    #region Style properties
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
            nameof(PaddingProperty),
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

/* usage:

<se:StswDataGridCheckColumn Header="Active" Binding="{Binding IsActive}"/>

*/
