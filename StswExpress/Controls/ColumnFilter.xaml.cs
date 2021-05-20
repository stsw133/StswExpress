using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ColumnFilter.xaml
    /// </summary>
    public partial class ColumnFilter : StackPanel
    {
        public ColumnFilter()
        {
            InitializeComponent();
        }

        public enum Type
        {
            Text,
            Number,
            Date,
            Bool,
            List
        }

        public enum Mode
        {
            Equal,
            NotEqual,
            Contains,
            NotContains,
            Like,
            NotLike,
            StartsWith,
            EndsWith,
            Greater,
            GreaterEqual,
            Less,
            LessEqual,
            Between
        }

        /// <summary>
        /// FilterMode
        /// </summary>
        public static readonly DependencyProperty FilterModeProperty
            = DependencyProperty.Register(
                  nameof(FilterMode),
                  typeof(Mode),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(Mode))
              );
        public Mode FilterMode
        {
            get => (Mode)GetValue(FilterModeProperty);
            set => SetValue(FilterModeProperty, value);
        }

        /// <summary>
        /// FilterType
        /// </summary>
        public static readonly DependencyProperty FilterTypeProperty
            = DependencyProperty.Register(
                  nameof(FilterType),
                  typeof(Type),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(Type))
              );
        public Type FilterType
        {
            get => (Type)GetValue(FilterTypeProperty);
            set => SetValue(FilterTypeProperty, value);
        }

        /// <summary>
        /// Header
        /// </summary>
        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
                  nameof(Header),
                  typeof(string),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(string))
              );
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// IsFilterVisible
        /// </summary>
        public static readonly DependencyProperty IsFilterVisibleProperty
            = DependencyProperty.Register(
                  nameof(IsFilterVisible),
                  typeof(bool),
                  typeof(ColumnFilter),
                  new PropertyMetadata(true)
              );
        public bool IsFilterVisible
        {
            get => (bool)GetValue(IsFilterVisibleProperty);
            set => SetValue(IsFilterVisibleProperty, value);
        }

        /// <summary>
        /// Value
        /// </summary>
        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register(
                  nameof(Value),
                  typeof(object),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(object))
              );
        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Loaded
        /// </summary>
        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var items = imgMode.ContextMenu.Items.OfType<MenuItem>().ToList();
            var binding = new Binding()
            {
                Path = new PropertyPath("Value"),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StackPanel), 1),
                TargetNullValue = "",
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            var inputbinding = new KeyBinding()
            {
                Command = Commands.Refresh,
                Key = Key.Return
            };
            var dp = imgMode.Parent as DockPanel;

            if (FilterType == Type.Text)
            {
                FilterMode = Mode.Contains;

                var valuecontainer = new TextBox();
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(TextBox.TextProperty, binding);
                dp.Children.Add(valuecontainer);

                items.First(x => x.Tag.ToString() == Mode.Greater.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.GreaterEqual.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.Less.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.LessEqual.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.Between.ToString()).Visibility = Visibility.Collapsed;
            }
            else if (FilterType == Type.Number)
            {
                FilterMode = Mode.Equal;

                var valuecontainer = new NumericUpDown();
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(NumericUpDown.ValueProperty, binding);
                dp.Children.Add(valuecontainer);

                items.First(x => x.Tag.ToString() == Mode.Contains.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.NotContains.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.Like.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.NotLike.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.StartsWith.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.EndsWith.ToString()).Visibility = Visibility.Collapsed;
            }
            imgMode.Source = new BitmapImage(new Uri($"pack://siteoforigin:,,,/Resources/icon32_filter_{FilterMode.ToString().ToLower()}.ico", UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Refresh
        /// </summary>
        private void cmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Commands.Refresh.Execute(null, Parent as UIElement);
            }
            catch { }
        }

        /// <summary>
        /// Filter mode click
        /// </summary>
        private void imgMode_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var c = sender as FrameworkElement;
            if (c != null) c.ContextMenu.IsOpen = true;
        }

        /// <summary>
        /// Filter mode change
        /// </summary>
        private void miFilterMode_Click(object sender, RoutedEventArgs e)
        {
            var items = imgMode.ContextMenu.Items.OfType<MenuItem>().ToList();
            FilterMode = (Mode)Enum.Parse(typeof(Mode), (sender as MenuItem).Tag.ToString());
            imgMode.Source = new BitmapImage(new Uri($"pack://siteoforigin:,,,/Resources/icon32_filter_{FilterMode.ToString().ToLower()}.ico", UriKind.RelativeOrAbsolute));
        }
    }
}
