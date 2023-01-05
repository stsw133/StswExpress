using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

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
        //Loaded += (s, e) => SetText();

        prop = GetType()?.GetProperty(nameof(SelectionBoxItem));
        prop = prop?.DeclaringType?.GetProperty(nameof(SelectionBoxItem));
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
              nameof(CornerRadius),
              typeof(CornerRadius?),
              typeof(StswComboBox),
              new PropertyMetadata(default(CornerRadius?))
          );
    public CornerRadius? CornerRadius
    {
        get => (CornerRadius?)GetValue(CornerRadiusProperty);
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

    /// PropertyChangedCallback
    public static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswComboBox comboBox && comboBox.IsLoaded)
        {
            //comboBox.Items;

            comboBox.SetText();
        }
    }

    /// SelectedItems
    public static readonly DependencyProperty SelectedItemsProperty
        = DependencyProperty.Register(
              nameof(SelectedItems),
              typeof(IList),
              typeof(StswComboBox),
              new FrameworkPropertyMetadata(new List<object>(),
                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                  ValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
          );
    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// ToggleButton_Click
    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var tgb = (ToggleButton)sender;
        var item = tgb.DataContext;

        var sameValues = false;
        foreach (var selectedItem in SelectedItems)
        {
            var differentValues = false;
            foreach (var property in selectedItem.GetType().GetProperties())
            {
                if (!property.GetValue(selectedItem).Equals(property.GetValue(item)))
                {
                    differentValues = true;
                    break;
                }
            }
            if (!differentValues)
                sameValues = true;
        }

        if (tgb.IsChecked == true && !sameValues)
            SelectedItems.Add(item);
        else if (tgb.IsChecked == false && sameValues)
            SelectedItems.Remove(item);

        SetText();
    }
    
    /// SetText
    private void SetText()
    {
        if (SelectedItems.Count > 1)
            prop?.SetValue(this, $"<{SelectedItems.Count} wybrano>", BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        else if (SelectedItems.Count == 0)
            prop?.SetValue(this, string.Empty, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        else
        {
            object? item = SelectedItems.OfType<object?>().FirstOrDefault();
            var displayProperty = item?.GetType()?.GetProperty(DisplayMemberPath);
            var display = displayProperty != null ? displayProperty.GetValue(item) : item?.ToString();
            prop?.SetValue(this, display ?? string.Empty, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        }
    }
}
