using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswDataGridComboColumn : DataGridComboBoxColumn
{
    public string? Format { get; set; }

    private static readonly Style StswDisplayElementStyle = new(typeof(StswComboBox), (Style)Application.Current.FindResource(typeof(StswComboBox)))
    {
        Setters =
        {
            new Setter(StswComboBox.BackgroundProperty, new SolidColorBrush(Colors.Transparent)),

            new Setter(StswComboBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswComboBox.CornerClippingProperty, false),
            new Setter(StswComboBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswComboBox.PaddingProperty, new Thickness(0)),
            new Setter(StswComboBox.SeparatorThicknessProperty, 0d),

            new Setter(StswComboBox.IsDropDownOpenProperty, false),

            new Setter(StswComboBox.FocusableProperty, false),
            new Setter(StswComboBox.IsDropDownOpenProperty, false),
            new Setter(StswComboBox.IsHitTestVisibleProperty, false),
            new Setter(StswComboBox.IsTabStopProperty, false),

            new Setter(StswComboBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswComboBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Left),
            new Setter(StswComboBox.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswComboBox.VerticalContentAlignmentProperty, VerticalAlignment.Top),

            new Setter(StswPopup.CornerClippingProperty, false),
            new Setter(StswPopup.CornerRadiusProperty, new CornerRadius(0))
        }
    };
    private static readonly Style StswEditingElementStyle = new(typeof(StswComboBox), (Style)Application.Current.FindResource(typeof(StswComboBox)))
    {
        Setters =
        {
            new Setter(StswComboBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswComboBox.CornerClippingProperty, false),
            new Setter(StswComboBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswComboBox.PaddingProperty, new Thickness(0)),
            new Setter(StswComboBox.SeparatorThicknessProperty, 0d),

            new Setter(StswComboBox.IsDropDownOpenProperty, true),

            new Setter(StswComboBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswComboBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Left),
            new Setter(StswComboBox.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswComboBox.VerticalContentAlignmentProperty, VerticalAlignment.Top),

            new Setter(StswPopup.CornerClippingProperty, false),
            new Setter(StswPopup.CornerRadiusProperty, new CornerRadius(0))
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
        cell.PreviewKeyDown += OnPreviewKeyDown;

        var displayElement = new StswComboBox()
        {
            ItemsSource = ItemsSource
        };

        /// bindings
        if (SelectedItemBinding != null)
            BindingOperations.SetBinding(displayElement, StswComboBox.SelectedItemProperty, SelectedItemBinding);
        if (SelectedValueBinding != null)
            BindingOperations.SetBinding(displayElement, StswComboBox.SelectedValueProperty, SelectedValueBinding);

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
        var editingElement = new StswComboBox()
        {
            ItemsSource = ItemsSource
        };

        /// bindings
        if (SelectedItemBinding != null)
            BindingOperations.SetBinding(editingElement, StswComboBox.SelectedItemProperty, SelectedItemBinding);
        if (SelectedValueBinding != null)
            BindingOperations.SetBinding(editingElement, StswComboBox.SelectedValueProperty, SelectedValueBinding);

        /// assign style
        editingElement.Style = StswEditingElementStyle;

        return editingElement;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
            return;

        if (sender is DataGridCell cell && !cell.IsEditing)
            cell.IsEditing = true;
    }
}