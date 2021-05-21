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
            Check,
            //Combo,
            Date,
            Number,
            Text
        }

        public enum Mode
        {
            Equal,
            NotEqual,
            Greater,
            GreaterEqual,
            Less,
            LessEqual,
            Between,
            Contains,
            NotContains,
            Like,
            NotLike,
            StartsWith,
            EndsWith
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
                  new PropertyMetadata(Type.Text)
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
        /// Value1
        /// </summary>
        public static readonly DependencyProperty Value1Property
            = DependencyProperty.Register(
                  nameof(Value1),
                  typeof(object),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(object))
              );
        public object Value1
        {
            get => GetValue(Value1Property);
            set => SetValue(Value1Property, value);
        }

        /// <summary>
        /// Value2
        /// </summary>
        public static readonly DependencyProperty Value2Property
            = DependencyProperty.Register(
                  nameof(Value2),
                  typeof(object),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(object))
              );
        public object Value2
        {
            get => GetValue(Value2Property);
            set => SetValue(Value2Property, value);
        }

        /// <summary>
        /// SQL
        /// </summary>
        public static readonly DependencyProperty NameSQLProperty
            = DependencyProperty.Register(
                  nameof(NameSQL),
                  typeof(string),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(string))
              );
        public string NameSQL
        {
            get => (string)GetValue(NameSQLProperty);
            set => SetValue(NameSQLProperty, value);
        }
        public string FilterSQL
        {
            get
            {
                var s = FilterType.In(Type.Text, Type.Date) ? "'" : "";
                switch (FilterMode)
                {
                    case Mode.Equal:
                        return $"{NameSQL} = {s}{Value1}{s}";
                    case Mode.NotEqual:
                        return $"{NameSQL} <> {s}{Value1}{s}";
                    case Mode.Greater:
                        return $"{NameSQL} > {s}{Value1}{s}";
                    case Mode.GreaterEqual:
                        return $"{NameSQL} >= {s}{Value1}{s}";
                    case Mode.Less:
                        return $"{NameSQL} < {s}{Value1}{s}";
                    case Mode.LessEqual:
                        return $"{NameSQL} <= {s}{Value1}{s}";
                    case Mode.Between:
                        return $"{NameSQL} between {s}{Value1}{s} and {s}{Value2}{s}";
                    case Mode.Contains:
                        return $"{NameSQL} like '%{Value1}%'";
                    case Mode.NotContains:
                        return $"{NameSQL} not like '%{Value1}%'";
                    case Mode.Like:
                        return $"{NameSQL} like '{Value1}'";
                    case Mode.NotLike:
                        return $"{NameSQL} not like '{Value1}'";
                    case Mode.StartsWith:
                        return $"{NameSQL} like '{Value1}%'";
                    case Mode.EndsWith:
                        return $"{NameSQL} like '%{Value1}'";
                    default:
                        throw new NotImplementedException();
                }
            }
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

            /// Check
            if (FilterType == Type.Check)
            {
                FilterMode = Mode.Equal;

                var valuecontainer = new ExtCheckBox()
                {
                    IsThreeState = true
                };
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(ExtCheckBox.IsCheckedProperty, binding);
                dp.Children.Add(valuecontainer);

                imgMode.Visibility = Visibility.Collapsed;
            }
            /// Date
            else if (FilterType == Type.Date)
            {
                FilterMode = Mode.Equal;

                var valuecontainer = new DatePicker();
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(DatePicker.SelectedDateProperty, binding);
                dp.Children.Add(valuecontainer);

                items.First(x => x.Tag.ToString() == Mode.Between.ToString()).Visibility = Visibility.Collapsed; //TEMP
                items.First(x => x.Tag.ToString() == Mode.Contains.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.NotContains.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.Like.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.NotLike.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.StartsWith.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.EndsWith.ToString()).Visibility = Visibility.Collapsed;
            }
            /// Number
            else if (FilterType == Type.Number)
            {
                FilterMode = Mode.Equal;

                var valuecontainer = new NumericUpDown();
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(NumericUpDown.ValueProperty, binding);
                dp.Children.Add(valuecontainer);

                items.First(x => x.Tag.ToString() == Mode.Between.ToString()).Visibility = Visibility.Collapsed; //TEMP
                items.First(x => x.Tag.ToString() == Mode.Contains.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.NotContains.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.Like.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.NotLike.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.StartsWith.ToString()).Visibility = Visibility.Collapsed;
                items.First(x => x.Tag.ToString() == Mode.EndsWith.ToString()).Visibility = Visibility.Collapsed;
            }
            /// Text
            else if (FilterType == Type.Text)
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
