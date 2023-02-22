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
public partial class StswComboBox : ComboBox
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
            prop?.SetValue(this, $"< {SelectedItems?.Count} >", BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
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

    #region Style
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswComboBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswComboBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswComboBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }

    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswComboBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswComboBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }

    /// BackgroundPressed
    public static readonly DependencyProperty BackgroundPressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundPressed),
            typeof(Brush),
            typeof(StswComboBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundPressed
    {
        get => (Brush)GetValue(BackgroundPressedProperty);
        set => SetValue(BackgroundPressedProperty, value);
    }
    /// BorderBrushPressed
    public static readonly DependencyProperty BorderBrushPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushPressed),
            typeof(Brush),
            typeof(StswComboBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushPressed
    {
        get => (Brush)GetValue(BorderBrushPressedProperty);
        set => SetValue(BorderBrushPressedProperty, value);
    }

    /// BackgroundReadOnly
    public static readonly DependencyProperty BackgroundReadOnlyProperty
        = DependencyProperty.Register(
            nameof(BackgroundReadOnly),
            typeof(Brush),
            typeof(StswComboBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundReadOnly
    {
        get => (Brush)GetValue(BackgroundReadOnlyProperty);
        set => SetValue(BackgroundReadOnlyProperty, value);
    }

    /// PopupBorderThickness
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswComboBox),
            new PropertyMetadata(default(Thickness))
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
            typeof(StswComboBox),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion

    #region Properties
    /// ButtonAlignment
    public static readonly DependencyProperty ButtonAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonAlignment),
            typeof(Dock),
            typeof(StswComboBox),
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
            typeof(StswComboBox),
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
            typeof(StswComboBox),
            new PropertyMetadata(default(SelectionMode))
        );
    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }
    #endregion
}
