using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress.Controls
{
    /// <summary>
    /// Interaction logic for MultiselectBox.xaml
    /// </summary>
    public partial class MultiselectBox : ComboBox
    {
        PropertyInfo prop;

        public MultiselectBox()
        {
            InitializeComponent();

            prop = GetType().GetProperty(nameof(SelectionBoxItem));
            prop = prop.DeclaringType.GetProperty(nameof(SelectionBoxItem));
        }

        /// <summary>
        /// Source
        /// </summary>
        public static readonly DependencyProperty SourceProperty
            = DependencyProperty.Register(
                  nameof(Source),
                  typeof(object[]),
                  typeof(MultiselectBox),
                  new PropertyMetadata(default(object[]))
              );
        public object[] Source
        {
            get => (object[])GetValue(SourceProperty);
            set
            {
                SetValue(SourceProperty, value);
                Items.Clear();
                for (int i = 0; i < Source.Length; i++)
                {
                    var tbtn = new ToggleButton()
                    {
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Content = Source[i]?.ToString(),
                        HorizontalContentAlignment = HorizontalAlignment.Left
                    };
                    tbtn.Click += ToggleButton_Click;
                    Items.Add(tbtn);
                }
            }
        }

        /// <summary>
        /// SelectedIndexes
        /// </summary>
        public static readonly DependencyProperty SelectedIndexesProperty
            = DependencyProperty.Register(
                  nameof(SelectedIndexesProperty),
                  typeof(int[]),
                  typeof(MultiselectBox),
                  new PropertyMetadata(default(int[]))
              );
        public int[] SelectedIndexes
        {
            get
            {
                var result = new List<int>();
                for (int i = 0; i < Items.Count; i++)
                    if ((Items[i] as ToggleButton).IsChecked == true)
                        result.Add(i);
                return result.ToArray();
            }
            set
            {
                SetValue(SelectedIndexesProperty, value);
                for (int i = 0; i < Items.Count; i++)
                {
                    if (value.Contains(i))
                        (Items[i] as ToggleButton).IsChecked = true;
                    else
                        (Items[i] as ToggleButton).IsChecked = false;
                }
                ToggleButton_Click(null, null);
            }
        }

        /// <summary>
        /// SelectedContents
        /// </summary>
        public object[] SelectedContents
        {
            get
            {
                var result = new List<object>();
                foreach (ToggleButton item in Items)
                    if (item.IsChecked == true)
                        result.Add(item.Content);
                return result.ToArray();
            }
        }

        /// <summary>
        /// Click
        /// </summary>
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            prop.SetValue(this, string.Join("; ", SelectedContents), BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        }
    }
}
