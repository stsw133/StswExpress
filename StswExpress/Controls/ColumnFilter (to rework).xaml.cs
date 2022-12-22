using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

/// <summary>
/// Interaction logic for ColumnFilter.xaml
/// </summary>
public partial class ColumnFilter : StackPanel
{
    public ColumnFilter()
    {
        InitializeComponent();
    }

    /// Modes
    public enum Modes
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
    /// Types
    public enum Types
    {
        Check,
        Date,
        Number,
        Text,
        ListOfNumbers,
        ListOfTexts
    }

    /// DisplayMemberPath
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

    /// FilterMode
    public static readonly DependencyProperty FilterModeProperty
        = DependencyProperty.Register(
              nameof(FilterMode),
              typeof(Modes?),
              typeof(ColumnFilter),
              new PropertyMetadata(default(Modes?))
          );
    public Modes? FilterMode
    {
        get => (Modes?)GetValue(FilterModeProperty);
        set
        {
            SetValue(FilterModeProperty, value);
            if (UniGriFilters.Children.Count >= 2)
                UniGriFilters.Children[1].Visibility = value == Modes.Between ? Visibility.Visible : Visibility.Collapsed;
            if (UniGriFilters.Children.Count >= 1)
                UniGriFilters.Children[0].Visibility = !value.In(Modes.Null, Modes.NotNull) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// FilterMinWidth
    public static readonly DependencyProperty FilterMinWidthProperty
        = DependencyProperty.Register(
              nameof(FilterMinWidth),
              typeof(double),
              typeof(ColumnFilter),
              new PropertyMetadata(50d)
          );
    public double FilterMinWidth
    {
        get => (double)GetValue(FilterMinWidthProperty);
        set => SetValue(FilterMinWidthProperty, value);
    }

    /// FilterSqlColumn
    public static readonly DependencyProperty FilterSqlColumnProperty
        = DependencyProperty.Register(
              nameof(FilterSqlColumn),
              typeof(string),
              typeof(ColumnFilter),
              new PropertyMetadata(default(string))
          );
    public string FilterSqlColumn
    {
        get => (string)GetValue(FilterSqlColumnProperty);
        set => SetValue(FilterSqlColumnProperty, value);
    }

    /// FilterType
    public static readonly DependencyProperty FilterTypeProperty
        = DependencyProperty.Register(
              nameof(FilterType),
              typeof(Types),
              typeof(ColumnFilter),
              new PropertyMetadata(Types.Text)
          );
    public Types FilterType
    {
        get => (Types)GetValue(FilterTypeProperty);
        set => SetValue(FilterTypeProperty, value);
    }

    /// FilterVisibility
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

    /// Header
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

    /// IsFilterCaseSensitive
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

    /// IsFilterNullSensitive
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

    /// ItemsHeaders
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

    /// ItemsSource
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

    /// SelectedValuePath
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

    /// StatsForeground
    public static readonly DependencyProperty StatsForegroundProperty
        = DependencyProperty.Register(
              nameof(StatsForeground),
              typeof(SolidColorBrush),
              typeof(ColumnFilter),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public SolidColorBrush StatsForeground
    {
        get => (SolidColorBrush)GetValue(StatsForegroundProperty);
        set => SetValue(StatsForegroundProperty, value);
    }

    /// StatsTooltip
    public static readonly DependencyProperty StatsTooltipProperty
        = DependencyProperty.Register(
              nameof(StatsTooltip),
              typeof(FrameworkElement),
              typeof(ColumnFilter),
              new PropertyMetadata(default(FrameworkElement))
          );
    public FrameworkElement StatsTooltip
    {
        get => (FrameworkElement)GetValue(StatsTooltipProperty);
        set => SetValue(StatsTooltipProperty, value);
    }

    /// StatsVisibility
    public static readonly DependencyProperty StatsVisibilityProperty
        = DependencyProperty.Register(
              nameof(StatsVisibility),
              typeof(Visibility),
              typeof(ColumnFilter),
              new PropertyMetadata(Visibility.Collapsed)
          );
    public Visibility StatsVisibility
    {
        get => (Visibility)GetValue(StatsVisibilityProperty);
        set => SetValue(StatsVisibilityProperty, value);
    }

    /// PropertyChangedCallback
    public static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is ColumnFilter filter)
            filter.This = filter;
    }

    /// Value1
    public static readonly DependencyProperty Value1Property
        = DependencyProperty.Register(
              nameof(Value1),
              typeof(object),
              typeof(ColumnFilter),
              new FrameworkPropertyMetadata(default(object),
                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                  ValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
          );
    public object Value1
    {
        get => GetValue(Value1Property);
        set
        {
            SetValue(Value1Property, FilterType == Types.Check ? value?.ToString().ToNullable<bool>() : value);
            if (FilterType.In(Types.ListOfNumbers, Types.ListOfTexts) && UniGriFilters.Children.Count > 0)
                ((MultiBox)UniGriFilters.Children[0]).SelectedItems = (List<object>)value;
        }
    }

    /// Value2
    public static readonly DependencyProperty Value2Property
        = DependencyProperty.Register(
              nameof(Value2),
              typeof(object),
              typeof(ColumnFilter),
              new FrameworkPropertyMetadata(default(object),
                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                  ValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
          );
    public object Value2
    {
        get => GetValue(Value2Property);
        set => SetValue(Value2Property, FilterType == Types.Check ? value?.ToString().ToNullable<bool>() : value);
    }

    /// ValueDef
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
            SetValue(ValueDefProperty, FilterType == Types.Check ? value?.ToString().ToNullable<bool>() : value);
            Value1 ??= FilterType == Types.Check ? value?.ToString().ToNullable<bool>() : value;
            Value2 ??= FilterType == Types.Check ? value?.ToString().ToNullable<bool>() : value;
        }
    }

    /// SqlParam
    public string SqlParam => "@" + new string(FilterSqlColumn.Where(char.IsLetterOrDigit).ToArray());

    /// SqlString
    public string? SqlString
    {
        get
        {
            if (Value1 == null || (Value1 is List<object> l && l.Count == 0))
                return null;
            if (Value2 == null && FilterMode == Modes.Between)
                return null;

            /// separator
            var s = FilterType.In(Types.Date, Types.Text, Types.ListOfTexts) ? "'" : string.Empty;
            /// case sensitive
            var cs1 = FilterType.In(Types.Text, Types.ListOfTexts) && !IsFilterCaseSensitive ? "lower(" : string.Empty;
            var cs2 = FilterType.In(Types.Text, Types.ListOfTexts) && !IsFilterCaseSensitive ? ")" : string.Empty;
            /// null sensitive
            var ns1 = !IsFilterNullSensitive ? "coalesce(" : string.Empty;
            var ns2 = string.Empty;
            if (!IsFilterNullSensitive)
            {
                ns2 = FilterType switch
                {
                    Types.Check => ", 0)",
                    Types.Date => ", '1900-01-01')",
                    Types.Number => ", 0)",
                    Types.Text => ", '')",
                    Types.ListOfNumbers => ", 0)",
                    Types.ListOfTexts => ", '')",
                    _ => string.Empty
                };
            }

            /// calculate SQL string
            return FilterMode switch
            {
                Modes.Equal => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} = {cs1}{SqlParam}1{cs2}",
                Modes.NotEqual => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} <> {cs1}{SqlParam}1{cs2}",
                Modes.Greater => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} > {cs1}{SqlParam}1{cs2}",
                Modes.GreaterEqual => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} >= {cs1}{SqlParam}1{cs2}",
                Modes.Less => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} < {cs1}{SqlParam}1{cs2}",
                Modes.LessEqual => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} <= {cs1}{SqlParam}1{cs2}",
                Modes.Between => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} between {cs1}{SqlParam}1{cs2} and {cs1}{SqlParam}2{cs2}",
                Modes.Contains => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} like {cs1}concat('%', {SqlParam}1, '%'){cs2}",
                Modes.NotContains => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} not like {cs1}concat('%', {SqlParam}1, '%'){cs2}",
                Modes.Like => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} like {cs1}{SqlParam}1{cs2}",
                Modes.NotLike => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} not like {cs1}{SqlParam}1{cs2}",
                Modes.StartsWith => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} like {cs1}concat({SqlParam}1, '%'){cs2}",
                Modes.EndsWith => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} like {cs1}concat('%', {SqlParam}1){cs2}",
                Modes.In => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", (List<object>)Value1)}{s}{cs2})",
                Modes.NotIn => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} not in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", (List<object>)Value1)}{s}{cs2})",
                Modes.Null => $"{FilterSqlColumn} is null)",
                Modes.NotNull => $"{FilterSqlColumn} is not null)",
                _ => null
            };
        }
    }

    /// This
    public static readonly DependencyProperty ThisProperty
        = DependencyProperty.Register(
              nameof(This),
              typeof(ColumnFilter),
              typeof(ColumnFilter),
              new FrameworkPropertyMetadata(default(ColumnFilter),
                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                  null, null, false, UpdateSourceTrigger.PropertyChanged)
          );
    public ColumnFilter This
    {
        get => (ColumnFilter)GetValue(ThisProperty);
        set => SetValue(ThisProperty, value);
    }

    /// Loaded
    private void StackPanel_Loaded(object sender, RoutedEventArgs e)
    {
        var items = ImgMode.ContextMenu.Items.OfType<MenuItem>().ToList();
        var binding1 = new Binding()
        {
            Path = new PropertyPath(nameof(Value1)),
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StackPanel), 1),
            TargetNullValue = string.Empty,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        var binding2 = new Binding()
        {
            Path = new PropertyPath(nameof(Value2)),
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StackPanel), 1),
            TargetNullValue = string.Empty,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        var bindingMinWidth = new Binding()
        {
            Path = new PropertyPath(nameof(FilterMinWidth)),
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StackPanel), 1),
            TargetNullValue = 0d,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        var inputbinding = new KeyBinding()
        {
            Command = StswCommands.Refresh,
            Key = Key.Return
        };

        /// Check
        if (FilterType == Types.Check)
        {
            var cont1 = new StswCheckBox()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                IsThreeState = true
            };
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(StswCheckBox.IsCheckedProperty, binding1);
            UniGriFilters.Children.Add(cont1);

            ImgMode.Visibility = Visibility.Collapsed;
        }
        /// Date
        else if (FilterType == Types.Date)
        {
            var cont1 = new StswDatePicker();
            cont1.Padding = new Thickness(0);
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(StswDatePicker.SelectedDateProperty, binding1);
            UniGriFilters.Children.Add(cont1);

            var cont2 = new StswDatePicker();
            cont2.Padding = new Thickness(0);
            cont2.InputBindings.Add(inputbinding);
            cont2.SetBinding(StswDatePicker.SelectedDateProperty, binding2);
            UniGriFilters.Children.Add(cont2);
        }
        /// List
        else if (FilterType.In(Types.ListOfNumbers, Types.ListOfTexts))
        {
            binding1.TargetNullValue = null;
            binding2.TargetNullValue = null;

            var cont1 = new MultiBox()
            {
                DisplayMemberPath = DisplayMemberPath,
                Padding = new Thickness(2),
                SelectedValuePath = SelectedValuePath,
                Source = ItemsHeaders?.ToString()?.Split(';')?.ToList() ?? ItemsSource
            };
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(MultiBox.SelectedItemsProperty, binding1);
            cont1.SetBinding(MultiBox.MinWidthProperty, bindingMinWidth);
            UniGriFilters.Children.Add(cont1);
        }
        /// Number
        else if (FilterType == Types.Number)
        {
            var cont1 = new StswNumericBox();
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(StswNumericBox.ValueProperty, binding1);
            cont1.SetBinding(MultiBox.MinWidthProperty, bindingMinWidth);
            UniGriFilters.Children.Add(cont1);

            var cont2 = new StswNumericBox();
            cont2.InputBindings.Add(inputbinding);
            cont2.SetBinding(StswNumericBox.ValueProperty, binding2);
            cont2.SetBinding(MultiBox.MinWidthProperty, bindingMinWidth);
            UniGriFilters.Children.Add(cont2);
        }
        /// Text
        else if (FilterType == Types.Text)
        {
            var cont1 = new StswTextBox();
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(StswTextBox.TextProperty, binding1);
            cont1.SetBinding(MultiBox.MinWidthProperty, bindingMinWidth);
            UniGriFilters.Children.Add(cont1);
        }

        /// Mode visibility
        items[(int)Modes.Equal].Visibility = FilterType.In(Types.Date, Types.Number, Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.NotEqual].Visibility = FilterType.In(Types.Date, Types.Number, Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.Greater].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.GreaterEqual].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.Less].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.LessEqual].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.Between].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.Contains].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.NotContains].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.Like].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.NotLike].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.StartsWith].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.EndsWith].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.In].Visibility = FilterType.In(Types.ListOfNumbers, Types.ListOfTexts) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.NotIn].Visibility = FilterType.In(Types.ListOfNumbers, Types.ListOfTexts) ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.Null].Visibility = IsFilterNullSensitive ? Visibility.Visible : Visibility.Collapsed;
        items[(int)Modes.NotNull].Visibility = IsFilterNullSensitive ? Visibility.Visible : Visibility.Collapsed;

        /// Shortcuts
        var keynumb = 1;
        foreach (var item in items.Where(x => x.Visibility == Visibility.Visible))
            if (!char.IsNumber(((string)item.Header)[2]))
                item.Header = "_" + keynumb++ + " " + item.Header.ToString();

        /// Default mode
        if (FilterMode == null)
        {
            if (FilterType == Types.Check) FilterMode = Modes.Equal;
            else if (FilterType == Types.Date) FilterMode = Modes.Equal;
            else if (FilterType.In(Types.ListOfNumbers, Types.ListOfTexts)) FilterMode = Modes.In;
            else if (FilterType == Types.Number) FilterMode = Modes.Equal;
            else if (FilterType == Types.Text) FilterMode = Modes.Contains;
        }
        /// Hide box filters
        if (UniGriFilters.Children.Count >= 2)
            UniGriFilters.Children[1].Visibility = FilterMode == Modes.Between ? Visibility.Visible : Visibility.Collapsed;
        if (UniGriFilters.Children.Count >= 1)
            UniGriFilters.Children[0].Visibility = !FilterMode.In(Modes.Null, Modes.NotNull) ? Visibility.Visible : Visibility.Collapsed;
        /// Set default value
        if (ValueDef != null)
        {
            if (FilterType.In(Types.ListOfNumbers, Types.ListOfTexts) && ValueDef is string def)
                ValueDef = def.Split(',').ToList<object>();
            if (Value1 == null) Value1 = ValueDef;
            if (Value2 == null) Value2 = ValueDef;
        }

        ImgMode.Source = new BitmapImage(new Uri($"pack://application:,,,/StswExpress;component/Icons/20_filter_{FilterMode?.ToString()?.ToLower()}.png"));

        Loaded -= StackPanel_Loaded;
    }

    /// Refresh
    private void CmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            StswCommands.Refresh.Execute(null, Parent as UIElement);
        }
        catch { }
    }

    /// Filter mode click
    private void ImgMode_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement c)
            c.ContextMenu.IsOpen = true;
    }

    /// Filter mode change
    private void MnuItmFilterMode_Click(object sender, RoutedEventArgs e)
    {
        FilterMode = (Modes)Enum.Parse(typeof(Modes), (string)((MenuItem)sender).Tag);
        ImgMode.Source = new BitmapImage(new Uri($"pack://application:,,,/StswExpress;component/Icons/20_filter_{FilterMode?.ToString().ToLower()}.png"));
    }
}
