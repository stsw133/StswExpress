using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress
{
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

        /// <summary>
        /// SelectedItems
        /// </summary>
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
                    var tbtn = (item.Content as ToggleButton);
                    if (tbtn.IsChecked == true)
                        result.Add(tbtn.Tag);
                }
                return result;
            }
            set
            {
                if (value is null || !(SelectedItems.Count == value.Count && SelectedItems.All(value.Contains)))
                {
                    foreach (ComboBoxItem item in Items)
                    {
                        var tbtn = (item.Content as ToggleButton);
                        tbtn.IsChecked = value?.Contains(tbtn.Tag) ?? false;
                    }
                    SetText();
                }
                SetValue(SelectedItemsProperty, value);
            }
        }

        /// <summary>
        /// Source
        /// </summary>
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
                for (int i = 0; i < value.Count; i++)
                {
                    var tbtn = new ToggleButton()
                    {
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Content = value[i].GetType().GetProperty(DisplayMemberPath).GetValue(value[i], null).ToString(),
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Tag = value[i].GetType().GetProperty(SelectedValuePath).GetValue(value[i], null).ToString()
                    };
                    tbtn.Click += ToggleButton_Click;

                    var cbi = new ComboBoxItem() { Content = tbtn };
                    Items.Add(cbi);
                }
            }
        }

        /// <summary>
        /// SetText
        /// </summary>
        private void SetText()
        {
            var result = new List<object>();
            foreach (ComboBoxItem item in Items)
            {
                var tbtn = (item.Content as ToggleButton);
                if (tbtn.IsChecked == true)
                    result.Add(tbtn.Content);
            }
            prop.SetValue(this, string.Join("; ", result), BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        }

        /// <summary>
        /// Click
        /// </summary>
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            SetText();
            SelectedItems = SelectedItems;
        }
    }
}
