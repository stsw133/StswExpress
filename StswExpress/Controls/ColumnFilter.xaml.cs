using System;
using System.Collections;
using System.Collections.Generic;
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
            Number,
            Text,
            ListOfNumbers,
            ListOfTexts
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
            NotIn,
            Null,
            NotNull
        }

        /// <summary>
        /// DisplayMemberPath
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty
            = DependencyProperty.Register(
                  nameof(DisplayMemberPath),
                  typeof(string),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(string))
              );
        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
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
                if (UniGriFilters.Children.Count >= 2)
                    UniGriFilters.Children[1].Visibility = value == Mode.Between ? Visibility.Visible : Visibility.Collapsed;
                if (UniGriFilters.Children.Count >= 1)
                    UniGriFilters.Children[0].Visibility = !value.In(Mode.Null, Mode.NotNull) ? Visibility.Visible : Visibility.Collapsed;
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
        /// FilterVisibility
        /// </summary>
        public static readonly DependencyProperty FilterVisibilityProperty
            = DependencyProperty.Register(
                  nameof(FilterVisibility),
                  typeof(Visibility),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(Visibility))
              );
        public Visibility FilterVisibility
        {
            get => (Visibility)GetValue(FilterVisibilityProperty);
            set => SetValue(FilterVisibilityProperty, value);
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
        /// ItemsHeaders
        /// </summary>
        public static readonly DependencyProperty ItemsHeadersProperty
            = DependencyProperty.Register(
                  nameof(ItemsHeaders),
                  typeof(object),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(object))
              );
        public object ItemsHeaders
        {
            get => GetValue(ItemsHeadersProperty);
            set => SetValue(ItemsHeadersProperty, value);
        }

        /// <summary>
        /// ItemsSource
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty
            = DependencyProperty.Register(
                  nameof(ItemsSource),
                  typeof(IList),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(IList))
              );
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// SelectedValuePath
        /// </summary>
        public static readonly DependencyProperty SelectedValuePathProperty
            = DependencyProperty.Register(
                  nameof(SelectedValuePath),
                  typeof(string),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(string))
              );
        public string SelectedValuePath
        {
            get => (string)GetValue(SelectedValuePathProperty);
            set => SetValue(SelectedValuePathProperty, value);
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
            set
            {
                SetValue(Value1Property, value);
                if (FilterType.In(Type.ListOfNumbers, Type.ListOfTexts) && UniGriFilters.Children.Count > 0)
                    (UniGriFilters.Children[0] as MultiBox).SelectedItems = value as List<object>;
            }
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
        /// ValueDef
        /// </summary>
        public static readonly DependencyProperty ValueDefProperty
            = DependencyProperty.Register(
                  nameof(ValueDef),
                  typeof(object),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(object))
              );
        public object ValueDef
        {
            get => GetValue(ValueDefProperty);
            set
            {
                SetValue(ValueDefProperty, value);
                if (Value1 == null) Value1 = value;
                if (Value2 == null) Value2 = value;
            }
        }

        /// <summary>
        /// NameSQL
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

        /// ParamSQL
        public string ParamSQL => "@" + new string(NameSQL.Where(char.IsLetterOrDigit).ToArray());

        /// SQL (filter)
        public string FilterSQL
        {
            get
            {
                if (Value1 == null || (Value1 is List<object> l && l.Count == 0))
                    return null;
                if (Value2 == null && FilterMode == Mode.Between)
                    return null;

                var s = FilterType.In(Type.Date, Type.Text, Type.ListOfTexts) ? "'" : string.Empty;
                var cs1 = FilterType.In(Type.Text, Type.ListOfTexts) && !IsFilterCaseSensitive ? "lower(" : string.Empty;
                var cs2 = FilterType.In(Type.Text, Type.ListOfTexts) && !IsFilterCaseSensitive ? ")" : string.Empty;
                var ns1 = !IsFilterNullSensitive ? "coalesce(" : string.Empty;
                var ns2 = string.Empty;
                if (!IsFilterNullSensitive)
                {
                    ns2 = FilterType switch
                    {
                        Type.Check => ", 0)",
                        Type.Date => ", '1900-01-01')",
                        Type.Number => ", 0)",
                        Type.Text => ", '')",
                        Type.ListOfNumbers => ", 0)",
                        Type.ListOfTexts => ", '')",
                        _ => string.Empty
                    };
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
                    Mode.In => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", (List<object>)Value1)}{s}{cs2})",
                    Mode.NotIn => $"{cs1}{ns1}{NameSQL}{ns2}{cs2} not in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", (List<object>)Value1)}{s}{cs2})",
                    Mode.Null => $"{NameSQL} is null)",
                    Mode.NotNull => $"{NameSQL} is not null)",
                    _ => null
                };
            }
        }

        /// Loaded
        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var items = ImgMode.ContextMenu.Items.OfType<MenuItem>().ToList();
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
                var cont1 = new ExtCheckBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsThreeState = true
                };
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(ExtCheckBox.IsCheckedProperty, binding1);
                UniGriFilters.Children.Add(cont1);

                ImgMode.Visibility = Visibility.Collapsed;
            }
            /// Date
            else if (FilterType == Type.Date)
            {
                var cont1 = new ExtDatePicker();
                cont1.Padding = new Thickness(0);
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(ExtDatePicker.SelectedDateProperty, binding1);
                UniGriFilters.Children.Add(cont1);

                var cont2 = new ExtDatePicker();
                cont2.Padding = new Thickness(0);
                cont2.InputBindings.Add(inputbinding);
                cont2.SetBinding(ExtDatePicker.SelectedDateProperty, binding2);
                UniGriFilters.Children.Add(cont2);
            }
            /// List
            else if (FilterType.In(Type.ListOfNumbers, Type.ListOfTexts))
            {
                binding1.TargetNullValue = null;
                binding2.TargetNullValue = null;

                var cont1 = new MultiBox()
                {
                    DisplayMemberPath = DisplayMemberPath,
                    Padding = new Thickness(2),
                    SelectedValuePath = SelectedValuePath,
                    Source = ItemsHeaders == null ? ItemsSource : ItemsHeaders.ToString().Split(';').ToList()
                };
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(MultiBox.SelectedItemsProperty, binding1);
                UniGriFilters.Children.Add(cont1);
            }
            /// Number
            else if (FilterType == Type.Number)
            {
                var cont1 = new NumericUpDown();
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(NumericUpDown.ValueProperty, binding1);
                UniGriFilters.Children.Add(cont1);

                var cont2 = new NumericUpDown();
                cont2.InputBindings.Add(inputbinding);
                cont2.SetBinding(NumericUpDown.ValueProperty, binding2);
                UniGriFilters.Children.Add(cont2);
            }
            /// Text
            else if (FilterType == Type.Text)
            {
                var cont1 = new ExtTextBox();
                cont1.InputBindings.Add(inputbinding);
                cont1.SetBinding(ExtTextBox.TextProperty, binding1);
                UniGriFilters.Children.Add(cont1);
            }

            /// Mode visibility
            items[(int)Mode.Equal].Visibility = FilterType.In(Type.Date, Type.Number, Type.Text) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.NotEqual].Visibility = FilterType.In(Type.Date, Type.Number, Type.Text) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.Greater].Visibility = FilterType.In(Type.Date, Type.Number) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.GreaterEqual].Visibility = FilterType.In(Type.Date, Type.Number) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.Less].Visibility = FilterType.In(Type.Date, Type.Number) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.LessEqual].Visibility = FilterType.In(Type.Date, Type.Number) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.Between].Visibility = FilterType.In(Type.Date, Type.Number) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.Contains].Visibility = FilterType.In(Type.Text) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.NotContains].Visibility = FilterType.In(Type.Text) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.Like].Visibility = FilterType.In(Type.Text) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.NotLike].Visibility = FilterType.In(Type.Text) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.StartsWith].Visibility = FilterType.In(Type.Text) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.EndsWith].Visibility = FilterType.In(Type.Text) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.In].Visibility = FilterType.In(Type.ListOfNumbers, Type.ListOfTexts) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.NotIn].Visibility = FilterType.In(Type.ListOfNumbers, Type.ListOfTexts) ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.Null].Visibility = IsFilterNullSensitive ? Visibility.Visible : Visibility.Collapsed;
            items[(int)Mode.NotNull].Visibility = IsFilterNullSensitive ? Visibility.Visible : Visibility.Collapsed;

            /// Shortcuts
            var keynumb = 1;
            foreach (var item in items.Where(x => x.Visibility == Visibility.Visible))
                if (!char.IsNumber(item.Header.ToString()[2]))
                    item.Header = "_" + keynumb++ + " " + item.Header.ToString();

            /// Default mode
            if (FilterMode == null)
            {
                if (FilterType == Type.Check) FilterMode = Mode.Equal;
                else if (FilterType == Type.Date) FilterMode = Mode.Equal;
                else if (FilterType.In(Type.ListOfNumbers, Type.ListOfTexts)) FilterMode = Mode.In;
                else if (FilterType == Type.Number) FilterMode = Mode.Equal;
                else if (FilterType == Type.Text) FilterMode = Mode.Contains;
            }
            /// Hide box filters
            if (UniGriFilters.Children.Count >= 2)
                UniGriFilters.Children[1].Visibility = FilterMode == Mode.Between ? Visibility.Visible : Visibility.Collapsed;
            if (UniGriFilters.Children.Count >= 1)
                UniGriFilters.Children[0].Visibility = !FilterMode.In(Mode.Null, Mode.NotNull) ? Visibility.Visible : Visibility.Collapsed;
            /// Set default value
            if (ValueDef != null)
            {
                if (FilterType.In(Type.ListOfNumbers, Type.ListOfTexts))
                    ValueDef = ValueDef.ToString().Split(',').ToList<object>();
                if (Value1 == null) Value1 = ValueDef;
                if (Value2 == null) Value2 = ValueDef;
            }

            ImgMode.Source = new BitmapImage(new Uri($"pack://application:,,,/StswExpress;component/Resources/icon20_filter_{FilterMode.ToString().ToLower()}.ico"));

            Loaded -= StackPanel_Loaded;
        }

        /// Refresh
        private void CmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Commands.Refresh.Execute(null, Parent as UIElement);
            }
            catch { }
        }

        /// Filter mode click
        private void ImgMode_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement c) c.ContextMenu.IsOpen = true;
        }

        /// Filter mode change
        private void MnuItmFilterMode_Click(object sender, RoutedEventArgs e)
        {
            FilterMode = (Mode)Enum.Parse(typeof(Mode), ((MenuItem)sender).Tag.ToString());
            ImgMode.Source = new BitmapImage(new Uri($"pack://application:,,,/StswExpress;component/Resources/icon20_filter_{FilterMode?.ToString().ToLower()}.ico"));
        }
    }
}
