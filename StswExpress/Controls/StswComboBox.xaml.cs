using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswComboBox.xaml
/// </summary>
public partial class StswComboBox : StswComboBoxBase
{
    private readonly PropertyInfo? prop;

    public StswComboBox()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            if (SelectionMode == SelectionMode.Multiple)
            {
                SelectedItems ??= new List<object>();
                SetText();
            }
        };

        prop = GetType()?.GetProperty(nameof(SelectionBoxItem));
        prop = prop?.DeclaringType?.GetProperty(nameof(SelectionBoxItem));
    }
    static StswComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComboBox), new FrameworkPropertyMetadata(typeof(StswComboBox)));
    }

    /// SelectedItems
    public static readonly DependencyProperty SelectedItemsProperty
        = DependencyProperty.Register(
              nameof(SelectedItems),
              typeof(IList),
              typeof(StswComboBox),
              new PropertyMetadata(new List<object>())
          );
    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// DoContainItem
    private bool DoContainItem(object item)
    {
        object? prop = null;
        if (!item?.GetType()?.IsValueType == true && item?.GetType() != typeof(string) && item?.GetType()?.GetProperty(SelectedValuePath) != null)
            prop = item.GetType()?.GetProperty(SelectedValuePath)?.GetValue(item);

        var found = false;
        foreach (var itm in SelectedItems)
        {
            if (prop != null)
            {
                var itmValue = itm.GetType()?.GetProperty(SelectedValuePath)?.GetValue(itm);
                if (itmValue?.Equals(prop) == true)
                {
                    found = true;
                    break;
                }
            }
            else
            {
                if (itm == item)
                {
                    found = true;
                    break;
                }
            }
        }

        return found;
    }

    /// ToggleButton_Loaded
    private void ToggleButton_Loaded(object sender, EventArgs e)
    {
        var tgb = (ToggleButton)sender;
        var item = tgb.DataContext;
        var found = DoContainItem(item);

        tgb.IsChecked = found;

        SetText();
    }

    /// ToggleButton_Click
    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var tgb = (ToggleButton)sender;
        var item = tgb.DataContext;
        var found = DoContainItem(item);

        if (tgb.IsChecked == true && !found)
            SelectedItems?.Add(item);
        else if (tgb.IsChecked == false && found)
        {
            object? itemToDelete = null;

            foreach (var itm in SelectedItems)
            {
                if (!itm?.GetType()?.IsValueType == true && itm?.GetType() != typeof(string))
                {
                    if (itm?.GetType()?.GetProperty(SelectedValuePath)?.GetValue(itm)?.Equals(item?.GetType()?.GetProperty(SelectedValuePath)?.GetValue(item)) == true)
                        itemToDelete = itm;
                }
                else if (itm == item)
                    itemToDelete = itm;
            }

            SelectedItems?.Remove(itemToDelete);
        }

        var newSelectedItems = new List<object>();
        foreach (var selectedItem in SelectedItems)
            newSelectedItems.Add(selectedItem);
        SelectedItems = newSelectedItems;

        SetText();
    }

    /// SetText
    internal void SetText()
    {
        if (SelectedItems?.Count > 1)
            prop?.SetValue(this, $"<{SelectedItems?.Count} wybrano>", BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        else if (SelectedItems?.Count == 0)
            prop?.SetValue(this, string.Empty, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        else
        {
            object? item = SelectedItems?.OfType<object?>()?.FirstOrDefault();

            if (!item?.GetType()?.IsValueType == true && item?.GetType() != typeof(string))
            {
                var displayProperty = item?.GetType()?.GetProperty(DisplayMemberPath);
                var display = displayProperty != null ? displayProperty.GetValue(item) : item?.ToString();
                prop?.SetValue(this, display ?? string.Empty, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            }
            else
                prop?.SetValue(this, item, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        }
    }
}

public class StswComboBoxBase : ComboBox
{
    #region StyleColors
    /// StyleColorDisabledBackground
    public static readonly DependencyProperty StyleColorDisabledBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledBackground),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorDisabledBackground
    {
        get => (Brush)GetValue(StyleColorDisabledBackgroundProperty);
        set => SetValue(StyleColorDisabledBackgroundProperty, value);
    }

    /// StyleColorDisabledBorder
    public static readonly DependencyProperty StyleColorDisabledBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledBorder),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorDisabledBorder
    {
        get => (Brush)GetValue(StyleColorDisabledBorderProperty);
        set => SetValue(StyleColorDisabledBorderProperty, value);
    }

    /// StyleColorMouseOverBackground
    public static readonly DependencyProperty StyleColorMouseOverBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorMouseOverBackground),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorMouseOverBackground
    {
        get => (Brush)GetValue(StyleColorMouseOverBackgroundProperty);
        set => SetValue(StyleColorMouseOverBackgroundProperty, value);
    }

    /// StyleColorMouseOverBorder
    public static readonly DependencyProperty StyleColorMouseOverBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorMouseOverBorder),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorMouseOverBorder
    {
        get => (Brush)GetValue(StyleColorMouseOverBorderProperty);
        set => SetValue(StyleColorMouseOverBorderProperty, value);
    }

    /// StyleColorPressedBackground
    public static readonly DependencyProperty StyleColorPressedBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorPressedBackground),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedBackground
    {
        get => (Brush)GetValue(StyleColorPressedBackgroundProperty);
        set => SetValue(StyleColorPressedBackgroundProperty, value);
    }

    /// StyleColorPressedBorder
    public static readonly DependencyProperty StyleColorPressedBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorPressedBorder),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedBorder
    {
        get => (Brush)GetValue(StyleColorPressedBorderProperty);
        set => SetValue(StyleColorPressedBorderProperty, value);
    }

    /// StyleColorReadOnlyBackground
    public static readonly DependencyProperty StyleColorReadOnlyBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorReadOnlyBackground),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorReadOnlyBackground
    {
        get => (Brush)GetValue(StyleColorReadOnlyBackgroundProperty);
        set => SetValue(StyleColorReadOnlyBackgroundProperty, value);
    }

    /// StyleColorReadOnlyBorder
    public static readonly DependencyProperty StyleColorReadOnlyBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorReadOnlyBorder),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorReadOnlyBorder
    {
        get => (Brush)GetValue(StyleColorReadOnlyBorderProperty);
        set => SetValue(StyleColorReadOnlyBorderProperty, value);
    }

    /// StyleThicknessSubBorder
    public static readonly DependencyProperty StyleThicknessSubBorderProperty
        = DependencyProperty.Register(
            nameof(StyleThicknessSubBorder),
            typeof(Thickness),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness StyleThicknessSubBorder
    {
        get => (Thickness)GetValue(StyleThicknessSubBorderProperty);
        set => SetValue(StyleThicknessSubBorderProperty, value);
    }
    #endregion

    /// ButtonAlignment
    public static readonly DependencyProperty ButtonAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonAlignment),
            typeof(Dock),
            typeof(StswComboBoxBase),
            new PropertyMetadata(Dock.Right)
        );
    public Dock ButtonAlignment
    {
        get => (Dock)GetValue(ButtonAlignmentProperty);
        set => SetValue(ButtonAlignmentProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// SelectionMode
    public static readonly DependencyProperty SelectionModeProperty
        = DependencyProperty.Register(
              nameof(SelectionMode),
              typeof(SelectionMode),
              typeof(StswComboBoxBase),
              new PropertyMetadata(default(SelectionMode))
          );
    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }
}
