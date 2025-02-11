using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
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
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dataItem"></param>
    /// <returns></returns>
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
    /// 
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
    /// 
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
    /// 
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