using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

public class StswToggleSelector : ComboBox
{
    public ICommand ComboBoxItemClickCommand { get; set; }

    public StswToggleSelector()
    {
        ComboBoxItemClickCommand = new StswRelayCommand<object>(ComboBoxItemClick);
    }
    static StswToggleSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToggleSelector), new FrameworkPropertyMetadata(typeof(StswToggleSelector)));
    }

    #region Events
    private PropertyInfo? prop;

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        Loaded += (s, e) =>
        {
            var comboBox = (ComboBox)s;
            comboBox.ApplyTemplate();

            if (GetTemplateChild("PART_Popup") is Popup popup and not null)
                popup.Opened += (s, args) =>
                {
                    for (int i = 0; i < comboBox.Items.Count; i++)
                        if (StswFn.FindVisualChild<ToggleButton>(comboBox.ItemContainerGenerator.ContainerFromIndex(i)) is ToggleButton button and not null)
                            button.IsChecked = DoContainItem(button.DataContext);
                };

            SelectedItems ??= new List<object>();
            SetText();
        };

        prop = GetType()?.GetProperty(nameof(SelectionBoxItem));
        prop = prop?.DeclaringType?.GetProperty(nameof(SelectionBoxItem));

        base.OnApplyTemplate();
    }

    /// DoContainItem
    internal bool DoContainItem(object item)
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

    /// ComboBoxItemClick
    private void ComboBoxItemClick(object? parameter)
    {
        if (parameter is ToggleButton button)
        {
            var item = button.DataContext;
            var found = DoContainItem(item);

            if (button.IsChecked == true && !found)
                SelectedItems?.Add(item);
            else if (button.IsChecked == false && found)
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
            /*
            var newSelectedItems = new List<object>();
            if (SelectedItems != null)
                foreach (var selectedItem in SelectedItems)
                    newSelectedItems.Add(selectedItem);
            SelectedItems = newSelectedItems;
            */
            SetText();
        }
    }

    /// SetText
    internal void SetText()
    {
        if (SelectedItems?.Count > 0)
        {
            var items = SelectedItems.OfType<object?>().ToList();
            var item = SelectedItems.OfType<object?>().FirstOrDefault();
            var listSep = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

            if (!item?.GetType()?.IsValueType == true && item?.GetType() != typeof(string))
            {
                var displayProperty = item?.GetType()?.GetProperty(DisplayMemberPath);
                var display = string.Join(listSep, items.Select(x => (displayProperty != null ? displayProperty.GetValue(x) : x)?.ToString()));
                prop?.SetValue(this, !string.IsNullOrEmpty(display) ? $"[{SelectedItems?.Count}] {display}" : string.Empty, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            }
            else prop?.SetValue(this, $"[{SelectedItems?.Count}] {string.Join(listSep, items)}", BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        }
        else prop?.SetValue(this, string.Empty, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
    }
    #endregion

    #region Properties
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<UIElement>),
            typeof(StswToggleSelector)
        );
    public ObservableCollection<UIElement> Buttons
    {
        get => (ObservableCollection<UIElement>)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }
    /// ButtonsAlignment
    public static readonly DependencyProperty ButtonsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonsAlignment),
            typeof(Dock),
            typeof(StswToggleSelector)
        );
    public Dock ButtonsAlignment
    {
        get => (Dock)GetValue(ButtonsAlignmentProperty);
        set => SetValue(ButtonsAlignmentProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswToggleSelector)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// Placeholder
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswToggleSelector)
        );
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// SelectedItems
    public static readonly DependencyProperty SelectedItemsProperty
        = DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList),
            typeof(StswToggleSelector)
        );
    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }
    #endregion

    #region Style
    /// > Background ...
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
        );
    public Brush ForegroundPressed
    {
        get => (Brush)GetValue(ForegroundPressedProperty);
        set => SetValue(ForegroundPressedProperty, value);
    }
    /// ForegroundPlaceholder
    public static readonly DependencyProperty ForegroundPlaceholderProperty
        = DependencyProperty.Register(
            nameof(ForegroundPlaceholder),
            typeof(Brush),
            typeof(StswToggleSelector)
        );
    public Brush ForegroundPlaceholder
    {
        get => (Brush)GetValue(ForegroundPlaceholderProperty);
        set => SetValue(ForegroundPlaceholderProperty, value);
    }

    /// > BorderThickness ...
    /// PopupBorderThickness
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswToggleSelector)
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
            typeof(StswToggleSelector)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion
}
