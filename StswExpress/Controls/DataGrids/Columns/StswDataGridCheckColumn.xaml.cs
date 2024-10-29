using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswDataGridCheckColumn : DataGridCheckBoxColumn
{
    public string? Format { get; set; }

    private static readonly Style StswDisplayElementStyle = new(typeof(StswCheckBox), (Style)Application.Current.FindResource(typeof(StswCheckBox)))
    {
        Setters =
        {
            new Setter(StswCheckBox.BorderThicknessProperty, new Thickness(1)),
            new Setter(StswCheckBox.CornerClippingProperty, false),
            new Setter(StswCheckBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswCheckBox.PaddingProperty, new Thickness(0)),

            new Setter(StswCheckBox.FocusableProperty, false),
            new Setter(StswCheckBox.IsHitTestVisibleProperty, false),
            new Setter(StswCheckBox.IsTabStopProperty, false),

            new Setter(StswCheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center),
            new Setter(StswCheckBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
            new Setter(StswCheckBox.VerticalAlignmentProperty, VerticalAlignment.Center),
            new Setter(StswCheckBox.VerticalContentAlignmentProperty, VerticalAlignment.Center)
        }
    };
    private static readonly Style StswEditingElementStyle = new(typeof(StswCheckBox), (Style)Application.Current.FindResource(typeof(StswCheckBox)))
    {
        Setters =
        {
            new Setter(StswCheckBox.BorderThicknessProperty, new Thickness(1)),
            new Setter(StswCheckBox.CornerClippingProperty, false),
            new Setter(StswCheckBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswCheckBox.PaddingProperty, new Thickness(0)),

            new Setter(StswCheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center),
            new Setter(StswCheckBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
            new Setter(StswCheckBox.VerticalAlignmentProperty, VerticalAlignment.Center),
            new Setter(StswCheckBox.VerticalContentAlignmentProperty, VerticalAlignment.Center)
        }
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var displayElement = new StswCheckBox();

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(displayElement, StswCheckBox.IsCheckedProperty, Binding);

        /// assign style
        displayElement.Style = StswDisplayElementStyle;

        return displayElement;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswCheckBox();

        /// bindings
        if (Binding != null)
            BindingOperations.SetBinding(editingElement, StswCheckBox.IsCheckedProperty, Binding);

        /// assign style
        editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }
}