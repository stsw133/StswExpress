using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Interaction logic for MultiBox.xaml
/// </summary>
public partial class MultiBox : ComboBox
{
    private PropertyInfo prop;

    public MultiBox()
    {
        InitializeComponent();

        prop = GetType().GetProperty(nameof(SelectionBoxItem));
        prop = prop.DeclaringType.GetProperty(nameof(SelectionBoxItem));
    }

    /// SelectedItems
    public static readonly DependencyProperty SelectedItemsProperty
            = DependencyProperty.Register(
                  nameof(SelectedItemsProperty),
                  typeof(List<object>),
                  typeof(MultiBox),
                  new PropertyMetadata(default(List<object>))
              );
    public List<object> SelectedItems
    {
        get
        {
            var result = new List<object>();
            foreach (ComboBoxItem item in Items)
            {
                var tbtn = item.Content as ExtToggleButton;
                if (tbtn.IsChecked == true)
                    result.Add(tbtn.Tag);
            }
            return result;
        }
        set
        {
            if (value == null || !(SelectedItems.Count == value.Count && SelectedItems.All(value.Contains)))
            {
                foreach (ComboBoxItem item in Items)
                {
                    var tbtn = item.Content as ExtToggleButton;
                    tbtn.IsChecked = value?.Contains(tbtn.Tag) ?? false;
                }
                SetText();
            }
            SetValue(SelectedItemsProperty, value);
        }
    }

    /// Source
    public static readonly DependencyProperty SourceProperty
            = DependencyProperty.Register(
                  nameof(SourceProperty),
                  typeof(IList),
                  typeof(MultiBox),
                  new PropertyMetadata(default(IList))
              );
    public IList Source
    {
        get => (IList)GetValue(SourceProperty);
        set
        {
            SetValue(SourceProperty, value);
            Items.Clear();
            if (value == null)
                return;

            for (int i = 0; i < value.Count; i++)
            {
                var tbtn = new ExtToggleButton()
                {
                    BorderThickness = new Thickness(0),
                    Content = new TextBlock()
                    {
                        Text = string.IsNullOrEmpty(DisplayMemberPath) ? value[i].ToString() : value[i].GetType().GetProperty(DisplayMemberPath).GetValue(value[i], null).ToString()
                    },
                    CornerRadius = 0,
                    ForMultiBox = true,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Tag = string.IsNullOrEmpty(SelectedValuePath) ? value[i].ToString() : value[i].GetType().GetProperty(SelectedValuePath).GetValue(value[i], null).ToString()
                };
                tbtn.Click += ToggleButton_Click;

                var cbi = new ComboBoxItem() { Content = tbtn };
                Items.Add(cbi);
            }
        }
    }

    /// Loaded
    private void ComboBox_Loaded(object sender, RoutedEventArgs e) => SetText();

    /// SetText
    private void SetText()
    {
        var result = new List<object>();
        foreach (ComboBoxItem item in Items)
        {
            var tbtn = (ExtToggleButton)item.Content;
            if (tbtn.IsChecked == true)
                result.Add(((TextBlock)tbtn.Content).Text);
        }
        prop.SetValue(this, result.Count > 1 ? $"<{result.Count} wybrano>" : result.Count == 1 ? result.First() : string.Empty, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
    }

    /// Click
    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        SetText();
        SelectedItems = SelectedItems;
    }
}
