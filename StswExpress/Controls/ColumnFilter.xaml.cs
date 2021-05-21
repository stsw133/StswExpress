using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            //Multiselect,
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
            EndsWith,
            In,
            NotIn
        }

        /// <summary>
        /// FilterMode
        /// </summary>
        public static readonly DependencyProperty FilterModeProperty
            = DependencyProperty.Register(
                  nameof(FilterMode),
                  typeof(Mode?),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(Mode?))
              );
        public Mode? FilterMode
        {
            get => (Mode?)GetValue(FilterModeProperty);
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
        /// IsFilterCaseSensitive
        /// </summary>
        public static readonly DependencyProperty IsFilterCaseSensitiveProperty
            = DependencyProperty.Register(
                  nameof(IsFilterCaseSensitive),
                  typeof(bool),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(bool))
              );
        public bool IsFilterCaseSensitive
        {
            get => (bool)GetValue(IsFilterCaseSensitiveProperty);
            set => SetValue(IsFilterCaseSensitiveProperty, value);
        }

        /// <summary>
        /// IsFilterNullSensitive
        /// </summary>
        public static readonly DependencyProperty IsFilterNullSensitiveProperty
            = DependencyProperty.Register(
                  nameof(IsFilterNullSensitive),
                  typeof(bool),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(bool))
              );
        public bool IsFilterNullSensitive
        {
            get => (bool)GetValue(IsFilterNullSensitiveProperty);
            set => SetValue(IsFilterNullSensitiveProperty, value);
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
        /// SQL (name)
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

        /// <summary>
        /// SQL (filter)
        /// </summary>
        public string FilterSQL
        {
            get
            {
                var s = FilterType.In(Type.Text, Type.Date) ? "'" : string.Empty;
                var cs1 = FilterType == Type.Text && !IsFilterCaseSensitive ? "lower(" : string.Empty;
                var cs2 = FilterType == Type.Text && !IsFilterCaseSensitive ? ")" : string.Empty;
                var ns1 = !IsFilterNullSensitive ? "coalesce(" : string.Empty;
                string ns2 = string.Empty;
                if (!IsFilterNullSensitive)
                {
                    if (FilterType == Type.Check)
                        ns2 = ", false)";
                    else if (FilterType == Type.Date)
                        ns2 = ", '0001-01-01')";
                    else if (FilterType == Type.Number)
                        ns2 = ", 0)";
                    else if (FilterType == Type.Text)
                        ns2 = ", '')";
                }

                if (Value1 == null)
                    return null;
                if (Value2 == null && FilterMode == Mode.Between)
                    return null;

                return FilterMode switch
                {
                    Mode.Equal => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} = {cs1}{s}{Value1}{s}{cs2}",
                    Mode.NotEqual => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} <> {cs1}{s}{Value1}{s}{cs2}",
                    Mode.Greater => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} > {cs1}{s}{Value1}{s}{cs2}",
                    Mode.GreaterEqual => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} >= {cs1}{s}{Value1}{s}{cs2}",
                    Mode.Less => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} < {cs1}{s}{Value1}{s}{cs2}",
                    Mode.LessEqual => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} <= {cs1}{s}{Value1}{s}{cs2}",
                    Mode.Between => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} between {cs1}{s}{Value1}{s}{cs2} and {cs1}{s}{Value2}{s}{cs2}",
                    Mode.Contains => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} like {cs1}{s}%{Value1}%{s}{cs2}",
                    Mode.NotContains => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} not like {cs1}{s}%{Value1}%{s}{cs2}",
                    Mode.Like => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} like {cs1}{s}{Value1}{s}{cs2}",
                    Mode.NotLike => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} not like {cs1}{s}{Value1}{s}{cs2}",
                    Mode.StartsWith => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} like {cs1}{s}{Value1}%{s}{cs2}",
                    Mode.EndsWith => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} like {cs1}{s}%{Value1}{s}{cs2}",
                    Mode.In => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", Value1.ToString().Split(","))}{s}{cs2})",
                    Mode.NotIn => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} not in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", Value1.ToString().Split(","))}{s}{cs2})",
                    _ => null
                };
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
                Path = new PropertyPath("Value1"),
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
                if (FilterMode == null)
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
                if (FilterMode == null)
                    FilterMode = Mode.Equal;

                var valuecontainer = new DatePicker();
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(DatePicker.SelectedDateProperty, binding);
                dp.Children.Add(valuecontainer);
            }
            /// Number
            else if (FilterType == Type.Number)
            {
                if (FilterMode == null)
                    FilterMode = Mode.Equal;

                var valuecontainer = new NumericUpDown();
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(NumericUpDown.ValueProperty, binding);
                dp.Children.Add(valuecontainer);
            }
            /// Text
            else if (FilterType == Type.Text)
            {
                if (FilterMode == null)
                    FilterMode = Mode.Contains;

                var valuecontainer = new TextBox();
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(TextBox.TextProperty, binding);
                dp.Children.Add(valuecontainer);
            }

            /// Mode visibility
            if (FilterType.In(Type.Check, Type.Text))
            {
                items[(int)Mode.Greater].Visibility = Visibility.Collapsed;
                items[(int)Mode.GreaterEqual].Visibility = Visibility.Collapsed;
                items[(int)Mode.Less].Visibility = Visibility.Collapsed;
                items[(int)Mode.LessEqual].Visibility = Visibility.Collapsed;
                items[(int)Mode.Between].Visibility = Visibility.Collapsed;
            }
            if (FilterType.In(Type.Check, Type.Date, Type.Number))
            {
                items[(int)Mode.Contains].Visibility = Visibility.Collapsed;
                items[(int)Mode.NotContains].Visibility = Visibility.Collapsed;
                items[(int)Mode.Like].Visibility = Visibility.Collapsed;
                items[(int)Mode.NotLike].Visibility = Visibility.Collapsed;
                items[(int)Mode.StartsWith].Visibility = Visibility.Collapsed;
                items[(int)Mode.EndsWith].Visibility = Visibility.Collapsed;
            }
            if (FilterType.In(Type.Check, Type.Date, Type.Number))
            {
                items[(int)Mode.In].Visibility = Visibility.Collapsed;
                items[(int)Mode.NotIn].Visibility = Visibility.Collapsed;
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
            if (sender is FrameworkElement c) c.ContextMenu.IsOpen = true;
        }

        /// <summary>
        /// Filter mode change
        /// </summary>
        private void miFilterMode_Click(object sender, RoutedEventArgs e)
        {
            FilterMode = (Mode)Enum.Parse(typeof(Mode), (sender as MenuItem).Tag.ToString());
            imgMode.Source = new BitmapImage(new Uri($"pack://siteoforigin:,,,/Resources/icon32_filter_{FilterMode.ToString().ToLower()}.ico", UriKind.RelativeOrAbsolute));
        }
    }
}
