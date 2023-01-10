using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;

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
            SelectedItems ??= new List<object>();
            SetText();
        };

        prop = GetType()?.GetProperty(nameof(SelectionBoxItem));
        prop = prop?.DeclaringType?.GetProperty(nameof(SelectionBoxItem));
    }
    static StswComboBox()
    {
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComboBox), new FrameworkPropertyMetadata(typeof(StswComboBox)));
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

        SetText();
    }

    /// SetText
    private void SetText()
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
