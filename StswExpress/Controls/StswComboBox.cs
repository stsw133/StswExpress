using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

public class StswComboBox : ComboBox
{
    static StswComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComboBox), new FrameworkPropertyMetadata(typeof(StswComboBox)));
    }

    #region Events
    private PropertyInfo? prop;

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        Loaded += (s, e) =>
        {
            if (SelectionMode == SelectionMode.Multiple && !IsEditable)
            {
                SelectedItems ??= new List<object>();
                SetText();
            }
        };

        prop = GetType()?.GetProperty(nameof(SelectionBoxItem));
        prop = prop?.DeclaringType?.GetProperty(nameof(SelectionBoxItem));

        base.OnApplyTemplate();
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
            prop?.SetValue(this, $"< {SelectedItems?.Count} wybrano >", BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
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
    #endregion

    #region Properties
    /// ButtonAlignment
    public static readonly DependencyProperty ButtonAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonAlignment),
            typeof(Dock),
            typeof(StswComboBox)
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
            typeof(StswComboBox)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// SelectedItems
    public static readonly DependencyProperty SelectedItemsProperty
        = DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList),
            typeof(StswComboBox)/*,
            new FrameworkPropertyMetadata(default(CornerRadius),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemsChanged, null, false, UpdateSourceTrigger.PropertyChanged)*/
        );
    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }
    public static void OnSelectedItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswComboBox stsw && !stsw.IsLoaded)
        {
            //stsw.AllowsTransparency = stsw.CornerRadius.TopLeft != 0;
        }
    }

    /// SelectionMode
    public static readonly DependencyProperty SelectionModeProperty
        = DependencyProperty.Register(
            nameof(SelectionMode),
            typeof(SelectionMode),
            typeof(StswComboBox)
        );
    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }
    #endregion

    #region Style
    /// > Background ...
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BackgroundPressed
    public static readonly DependencyProperty BackgroundPressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundPressed),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush BackgroundPressed
    {
        get => (Brush)GetValue(BackgroundPressedProperty);
        set => SetValue(BackgroundPressedProperty, value);
    }
    /// BackgroundReadOnly
    public static readonly DependencyProperty BackgroundReadOnlyProperty
        = DependencyProperty.Register(
            nameof(BackgroundReadOnly),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush BackgroundReadOnly
    {
        get => (Brush)GetValue(BackgroundReadOnlyProperty);
        set => SetValue(BackgroundReadOnlyProperty, value);
    }

    /// > BorderBrush ...
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// BorderBrushPressed
    public static readonly DependencyProperty BorderBrushPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushPressed),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush BorderBrushPressed
    {
        get => (Brush)GetValue(BorderBrushPressedProperty);
        set => SetValue(BorderBrushPressedProperty, value);
    }

    /// > Foreground ...
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }
    /// ForegroundMouseOver
    public static readonly DependencyProperty ForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ForegroundMouseOver),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
    }
    /// ForegroundPressed
    public static readonly DependencyProperty ForegroundPressedProperty
        = DependencyProperty.Register(
            nameof(ForegroundPressed),
            typeof(Brush),
            typeof(StswComboBox)
        );
    public Brush ForegroundPressed
    {
        get => (Brush)GetValue(ForegroundPressedProperty);
        set => SetValue(ForegroundPressedProperty, value);
    }

    /// > BorderThickness ...
    /// PopupBorderThickness
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswComboBox)
        );
    public Thickness PopupBorderThickness
    {
        get => (Thickness)GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswComboBox)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion
}
