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
            Date,
            List,
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
            set
            {
                SetValue(FilterModeProperty, value);
                if (ugFilters.Children.Count >= 2)
                    ugFilters.Children[1].Visibility = value == Mode.Between ? Visibility.Visible : Visibility.Collapsed;
            }
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
        public string ParamSQL => "@" + new string(NameSQL.Where(char.IsLetterOrDigit).ToArray());

        /// <summary>
        /// SQL (filter)
        /// </summary>
        public string FilterSQL
        {
            get
            {
                if (Value1 == null)
                    return null;
                if (Value2 == null && FilterMode == Mode.Between)
                    return null;

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

                return FilterMode switch
                {
                    Mode.Equal => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} = {cs1}{ParamSQL}1{cs2}",
                    Mode.NotEqual => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} <> {cs1}{ParamSQL}1{cs2}",
                    Mode.Greater => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} > {cs1}{ParamSQL}1{cs2}",
                    Mode.GreaterEqual => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} >= {cs1}{ParamSQL}1{cs2}",
                    Mode.Less => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} < {cs1}{ParamSQL}1{cs2}",
                    Mode.LessEqual => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} <= {cs1}{ParamSQL}1{cs2}",
                    Mode.Between => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} between {cs1}{ParamSQL}1{cs2} and {cs1}{ParamSQL}2{cs2}",
                    Mode.Contains => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} like {cs1}concat('%', {ParamSQL}1, '%'){cs2}",
                    Mode.NotContains => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} not like {cs1}concat('%', {ParamSQL}1, '%'){cs2}",
                    Mode.Like => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} like {cs1}{ParamSQL}1{cs2}",
                    Mode.NotLike => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} not like {cs1}{ParamSQL}1{cs2}",
                    Mode.StartsWith => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} like {cs1}concat({ParamSQL}1, '%'){cs2}",
                    Mode.EndsWith => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} like {cs1}concat('%', {ParamSQL}1){cs2}",
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
            var binding1 = new Binding()
            {
                Path = new PropertyPath("Value1"),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StackPanel), 1),
                TargetNullValue = "",
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            var binding2 = new Binding()
            {
                Path = new PropertyPath("Value2"),
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

            /// Check
            if (FilterType == Type.Check)
            {
                var cont1 = new ExtCheckBox() { IsThreeState = true };
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(ExtCheckBox.IsCheckedProperty, binding1);
                ugFilters.Children.Add(cont1);

                imgMode.Visibility = Visibility.Collapsed;
            }
            /// Date
            else if (FilterType == Type.Date)
            {
                var cont1 = new DatePicker();
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(DatePicker.SelectedDateProperty, binding1);
                ugFilters.Children.Add(cont1);

                var cont2 = new DatePicker();
                cont2.InputBindings.Add(inputbinding);
                cont2.SetBinding(DatePicker.SelectedDateProperty, binding2);
                ugFilters.Children.Add(cont2);
            }
            /// List
            else if (FilterType == Type.List)
            {
                var cont1 = new MultiselectBox();
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(MultiselectBox.StringOfContentsProperty, binding1);
                ugFilters.Children.Add(cont1);
            }
            /// Number
            else if (FilterType == Type.Number)
            {
                var cont1 = new NumericUpDown();
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(NumericUpDown.ValueProperty, binding1);
                ugFilters.Children.Add(cont1);

                var cont2 = new NumericUpDown();
                cont2.InputBindings.Add(inputbinding);
                cont2.SetBinding(NumericUpDown.ValueProperty, binding2);
                ugFilters.Children.Add(cont2);
            }
            /// Text
            else if (FilterType == Type.Text)
            {
                var cont1 = new ExtTextBox();
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(ExtTextBox.TextProperty, binding1);
                ugFilters.Children.Add(cont1);
            }

            /// Mode visibility
            if (!FilterType.In(Type.Date, Type.Number))
            {
                items[(int)Mode.Greater].Visibility = Visibility.Collapsed;
                items[(int)Mode.GreaterEqual].Visibility = Visibility.Collapsed;
                items[(int)Mode.Less].Visibility = Visibility.Collapsed;
                items[(int)Mode.LessEqual].Visibility = Visibility.Collapsed;
                items[(int)Mode.Between].Visibility = Visibility.Collapsed;
            }
            if (!FilterType.In(Type.List))
            {
                items[(int)Mode.In].Visibility = Visibility.Collapsed;
                items[(int)Mode.NotIn].Visibility = Visibility.Collapsed;
            }
            if (!FilterType.In(Type.Text))
            {
                items[(int)Mode.Contains].Visibility = Visibility.Collapsed;
                items[(int)Mode.NotContains].Visibility = Visibility.Collapsed;
                items[(int)Mode.Like].Visibility = Visibility.Collapsed;
                items[(int)Mode.NotLike].Visibility = Visibility.Collapsed;
                items[(int)Mode.StartsWith].Visibility = Visibility.Collapsed;
                items[(int)Mode.EndsWith].Visibility = Visibility.Collapsed;
            }

            /// Default mode
            if (FilterMode == null)
            {
                if      (FilterType == Type.Check)  FilterMode = Mode.Equal;
                else if (FilterType == Type.Date)   FilterMode = Mode.Equal;
                else if (FilterType == Type.List)   FilterMode = Mode.In;
                else if (FilterType == Type.Number) FilterMode = Mode.Equal;
                else if (FilterType == Type.Text)   FilterMode = Mode.Contains;
            }
            if (ugFilters.Children.Count >= 2)
                ugFilters.Children[1].Visibility = FilterMode == Mode.Between ? Visibility.Visible : Visibility.Collapsed;

            imgMode.Source = new BitmapImage(new Uri($"pack://siteoforigin:,,,/Resources/icon32_filter_{FilterMode.ToString().ToLower()}.ico", UriKind.RelativeOrAbsolute));

            Loaded -= StackPanel_Loaded;
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
