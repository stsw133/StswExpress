using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswColumnFilter.xaml
/// </summary>
public partial class StswColumnFilter : StswColumnFilterBase
{
    public StswColumnFilter()
    {
        InitializeComponent();
    }
    static StswColumnFilter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColumnFilter), new FrameworkPropertyMetadata(typeof(StswColumnFilter)));
    }

    /// FilterModeChanged
    public static void FilterModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColumnFilter filter)
        {
            var panel = filter.GetTemplateChild("UngFilters") as Panel;
            if (panel?.Children?.Count >= 2)
                panel.Children[1].Visibility = (Modes)e.NewValue == Modes.Between ? Visibility.Visible : Visibility.Collapsed;
            if (panel?.Children?.Count >= 1)
                panel.Children[0].Visibility = !((Modes)e.NewValue).In(Modes.Null, Modes.NotNull) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// FilterMode
    public static readonly DependencyProperty FilterModeProperty
        = DependencyProperty.Register(
            nameof(FilterMode),
            typeof(Modes?),
            typeof(StswColumnFilterBase),
            new FrameworkPropertyMetadata(default(Modes?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                FilterModeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public Modes? FilterMode
    {
        get => (Modes?)GetValue(FilterModeProperty);
        set => SetValue(FilterModeProperty, value);
    }
    internal Modes? DefaultFilterMode { get; set; } = null;
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

    /// FilterSqlColumn
    public static readonly DependencyProperty FilterSqlColumnProperty
        = DependencyProperty.Register(
            nameof(FilterSqlColumn),
            typeof(string),
            typeof(StswColumnFilter),
            new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                FilterSqlColumnChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public string FilterSqlColumn
    {
        get => (string)GetValue(FilterSqlColumnProperty);
        set => SetValue(FilterSqlColumnProperty, value);
    }

    /// FilterSqlColumnChanged
    public static void FilterSqlColumnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColumnFilter filter)
        {
            filter.SqlParam = "@" + new string(((string)e.NewValue).Where(char.IsLetterOrDigit).ToArray());
            filter.SetData();
        }
    }

    /// Value1
    public static readonly DependencyProperty Value1Property
        = DependencyProperty.Register(
            nameof(Value1),
            typeof(object),
            typeof(StswColumnFilter),
            new FrameworkPropertyMetadata(default(object?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                ValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public object? Value1
    {
        get => (object?)GetValue(Value1Property);
        set => SetValue(Value1Property, FilterType == Types.Check ? value?.ToString()?.ToNullable<bool>() : value);
    }
    internal object? DefaultValue1 { get; set; } = null;

    /// Value2
    public static readonly DependencyProperty Value2Property
        = DependencyProperty.Register(
            nameof(Value2),
            typeof(object),
            typeof(StswColumnFilter),
            new FrameworkPropertyMetadata(default(object?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                ValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public object? Value2
    {
        get => (object?)GetValue(Value2Property);
        set => SetValue(Value2Property, FilterType == Types.Check ? value?.ToString()?.ToNullable<bool>() : value);
    }
    internal object? DefaultValue2 { get; set; } = null;

    /// ValueChanged
    public static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColumnFilter filter)
        {
            var panel = filter.GetTemplateChild("UngFilters") as Panel;
            if (filter.FilterType.In(Types.ListOfNumbers, Types.ListOfTexts) && panel?.Children?.Count > 0)
                ((StswComboBox)panel.Children[0]).SelectedItems = (List<object>)e.NewValue;

            if (filter.Value1 == null || (filter.Value1 is List<object> l && l.Count == 0)
            ||  (filter.Value2 == null && filter.FilterMode == Modes.Between))
            {
                filter.SqlString = null;
                return;
            }

            /// separator
            var s = filter.FilterType.In(Types.Date, Types.Text, Types.ListOfTexts) ? "'" : string.Empty;
            /// case sensitive
            var cs1 = filter.FilterType.In(Types.Text, Types.ListOfTexts) && !filter.IsFilterCaseSensitive ? "lower(" : string.Empty;
            var cs2 = filter.FilterType.In(Types.Text, Types.ListOfTexts) && !filter.IsFilterCaseSensitive ? ")" : string.Empty;
            /// null sensitive
            var ns1 = !filter.IsFilterNullSensitive ? "coalesce(" : string.Empty;
            var ns2 = string.Empty;
            if (!filter.IsFilterNullSensitive)
            {
                ns2 = filter.FilterType switch
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
            filter.SqlString = filter.FilterMode switch
            {
                Modes.Equal => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} = {cs1}{filter.SqlParam}1{cs2}",
                Modes.NotEqual => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} <> {cs1}{filter.SqlParam}1{cs2}",
                Modes.Greater => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} > {cs1}{filter.SqlParam}1{cs2}",
                Modes.GreaterEqual => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} >= {cs1}{filter.SqlParam}1{cs2}",
                Modes.Less => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} < {cs1}{filter.SqlParam}1{cs2}",
                Modes.LessEqual => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} <= {cs1}{filter.SqlParam}1{cs2}",
                Modes.Between => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} between {cs1}{filter.SqlParam}1{cs2} and {cs1}{filter.SqlParam}2{cs2}",
                Modes.Contains => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} like {cs1}concat('%', {filter.SqlParam}1, '%'){cs2}",
                Modes.NotContains => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} not like {cs1}concat('%', {filter.SqlParam}1, '%'){cs2}",
                Modes.Like => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} like {cs1}{filter.SqlParam}1{cs2}",
                Modes.NotLike => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} not like {cs1}{filter.SqlParam}1{cs2}",
                Modes.StartsWith => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} like {cs1}concat({filter.SqlParam}1, '%'){cs2}",
                Modes.EndsWith => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} like {cs1}concat('%', {filter.SqlParam}1){cs2}",
                Modes.In => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", (List<object>)filter.Value1)}{s}{cs2})",
                Modes.NotIn => $"{cs1}{ns1}{filter.FilterSqlColumn}{ns2}{cs2} not in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", (List<object>)filter.Value1)}{s}{cs2})",
                Modes.Null => $"{filter.FilterSqlColumn} is null)",
                Modes.NotNull => $"{filter.FilterSqlColumn} is not null)",
                _ => null
            };

            filter.SetData();
        }
    }

    /// SqlParam
    public static readonly DependencyProperty SqlParamProperty
        = DependencyProperty.Register(
            nameof(SqlParam),
            typeof(string),
            typeof(StswColumnFilter),
            new PropertyMetadata(default(string))
        );
    public string SqlParam
    {
        get => (string)GetValue(SqlParamProperty);
        private set => SetValue(SqlParamProperty, value);
    }
    
    /// SqlString
    public static readonly DependencyProperty SqlStringProperty
        = DependencyProperty.Register(
            nameof(SqlString),
            typeof(string),
            typeof(StswColumnFilter),
            new PropertyMetadata(default(string?))
        );
    public string? SqlString
    {
        get => (string?)GetValue(SqlStringProperty);
        private set => SetValue(SqlStringProperty, value);
    }

    /// Data
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.Register(
            nameof(Data),
            typeof(StswColumnFilterData),
            typeof(StswColumnFilter),
            new PropertyMetadata(default(StswColumnFilterData?))
        );
    public StswColumnFilterData? Data
    {
        get => (StswColumnFilterData?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// SetData
    private void SetData()
    {
        Data = new StswColumnFilterData()
        {
            SqlParam = SqlParam,
            SqlString = SqlString,
            Value1 = Value1,
            Value2 = Value2,
            DefaultValue1 = DefaultValue1,
            DefaultValue2 = DefaultValue2,
            Clear = new Action(Clear)
        };
    }

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        var btnSymbol = (ButtonBase)GetTemplateChild("BtnSymbol");
        var ungFilters = (UniformGrid)GetTemplateChild("UngFilters");

        /// bindings for Value1 and Value2
        var btnSymbolItems = btnSymbol.ContextMenu.Items.OfType<MenuItem>().ToList();
        var binding1 = new Binding()
        {
            Path = new PropertyPath(nameof(Value1)),
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StswColumnFilterBase), 1),
            TargetNullValue = string.Empty,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        var binding2 = new Binding()
        {
            Path = new PropertyPath(nameof(Value2)),
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StswColumnFilterBase), 1),
            TargetNullValue = string.Empty,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        var subborderThickness = new Binding()
        {
            Path = new PropertyPath(nameof(BorderThickness) + ".Top"),
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StswColumnFilterBase), 1),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Converter = new conv_Multiply(),
            ConverterParameter = "0.5 0 0 0"
        };
        /*
        var bindingMinWidth = new Binding()
        {
            Path = new PropertyPath(nameof(FilterMinWidth)),
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StswColumnFilterBase), 1),
            TargetNullValue = 0d,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        */
        var inputbinding = new KeyBinding()
        {
            Command = StswCommands.Refresh,
            Key = Key.Return
        };

        /// create StswCheckBox when Check
        if (FilterType == Types.Check)
        {
            var cont1 = new StswCheckBox()
            {
                BorderThickness = new Thickness(0),
                Content = null,
                CornerRadius = new CornerRadius(0),
                IsThreeState = true
            };
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(StswCheckBox.IsCheckedProperty, binding1);
            ungFilters.Children.Add(cont1);
        }
        /// create StswDatePicker when Date
        else if (FilterType == Types.Date)
        {
            var cont1 = new StswDatePicker()
            {
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(StswDatePicker.SelectedDateProperty, binding1);
            cont1.SetBinding(StswDatePicker.StyleThicknessSubBorderProperty, subborderThickness);
            ungFilters.Children.Add(cont1);

            var cont2 = new StswDatePicker()
            {
                //BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            cont2.BorderThickness = new Thickness(0, cont2.BorderThickness.Top, 0, 0);
            cont2.InputBindings.Add(inputbinding);
            cont2.SetBinding(StswDatePicker.SelectedDateProperty, binding2);
            cont1.SetBinding(StswDatePicker.StyleThicknessSubBorderProperty, subborderThickness);
            ungFilters.Children.Add(cont2);
        }
        /// create StswComboBox when List
        else if (FilterType.In(Types.ListOfNumbers, Types.ListOfTexts))
        {
            binding1.TargetNullValue = null;
            binding2.TargetNullValue = null;

            var cont1 = new StswComboBox()
            {
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                DisplayMemberPath = DisplayMemberPath,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemsSource = /*ItemsHeaders?.ToString()?.Split(';')?.ToList() ??*/ ItemsSource,
                SelectionMode = SelectionMode.Multiple,
                SelectedValuePath = SelectedValuePath
            };
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(StswComboBox.SelectedItemsProperty, binding1);
            cont1.SetBinding(StswComboBox.StyleThicknessSubBorderProperty, subborderThickness);
            //cont1.SetBinding(StswComboBox.MinWidthProperty, bindingMinWidth);
            ungFilters.Children.Add(cont1);
        }
        /// create StswNumericBox when Number
        else if (FilterType == Types.Number)
        {
            var cont1 = new StswNumericBox()
            {
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(StswNumericBox.ValueProperty, binding1);
            cont1.SetBinding(StswNumericBox.StyleThicknessSubBorderProperty, subborderThickness);
            //cont1.SetBinding(StswComboBox.MinWidthProperty, bindingMinWidth);
            ungFilters.Children.Add(cont1);

            var cont2 = new StswNumericBox()
            {
                //BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            cont2.BorderThickness = new Thickness(0, cont2.BorderThickness.Top, 0, 0);
            cont2.InputBindings.Add(inputbinding);
            cont2.SetBinding(StswNumericBox.ValueProperty, binding2);
            cont1.SetBinding(StswNumericBox.StyleThicknessSubBorderProperty, subborderThickness);
            //cont2.SetBinding(StswComboBox.MinWidthProperty, bindingMinWidth);
            ungFilters.Children.Add(cont2);
        }
        /// create StswTextBox when Tect
        else if (FilterType == Types.Text)
        {
            var cont1 = new StswTextBox()
            {
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            cont1.InputBindings.Add(inputbinding);
            cont1.SetBinding(StswTextBox.TextProperty, binding1);
            //cont1.SetBinding(StswComboBox.MinWidthProperty, bindingMinWidth);
            ungFilters.Children.Add(cont1);
        }

        /// visibility for FilterMode items
        btnSymbolItems[(int)Modes.Equal].Visibility = FilterType.In(Types.Check, Types.Date, Types.Number, Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.NotEqual].Visibility = FilterType.In(Types.Date, Types.Number, Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.Greater].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.GreaterEqual].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.Less].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.LessEqual].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.Between].Visibility = FilterType.In(Types.Date, Types.Number) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.Contains].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.NotContains].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.Like].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.NotLike].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.StartsWith].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.EndsWith].Visibility = FilterType.In(Types.Text) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.In].Visibility = FilterType.In(Types.ListOfNumbers, Types.ListOfTexts) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.NotIn].Visibility = FilterType.In(Types.ListOfNumbers, Types.ListOfTexts) ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.Null].Visibility = IsFilterNullSensitive ? Visibility.Visible : Visibility.Collapsed;
        btnSymbolItems[(int)Modes.NotNull].Visibility = IsFilterNullSensitive ? Visibility.Visible : Visibility.Collapsed;

        /// shortcuts for FilterMode items
        var keynumb = 1;
        foreach (var item in btnSymbolItems.Where(x => x.Visibility == Visibility.Visible))
            if (!char.IsNumber(((string)item.Header)[2]))
                item.Header = "_" + keynumb++ + " " + item.Header.ToString();

        /// default FilterMode
        FilterMode ??= FilterType switch
        {
            Types.Check => Modes.Equal,
            Types.Date => Modes.Equal,
            Types.ListOfNumbers => Modes.In,
            Types.ListOfTexts => Modes.In,
            Types.Number => Modes.Equal,
            Types.Text => Modes.Contains,
            _ => null
        };

        /// hide box filters depending on FilterMode
        if (ungFilters.Children.Count >= 2)
            ungFilters.Children[1].Visibility = FilterMode == Modes.Between ? Visibility.Visible : Visibility.Collapsed;
        if (ungFilters.Children.Count >= 1)
            ungFilters.Children[0].Visibility = !FilterMode.In(Modes.Null, Modes.NotNull) ? Visibility.Visible : Visibility.Collapsed;

        /// set default values
        DefaultFilterMode = FilterMode;
        DefaultValue1 = Value1;
        DefaultValue2 = Value2;

        MnuItmFilterMode_Click(btnSymbolItems[(int)(FilterMode ?? 0)], null);

        base.OnApplyTemplate();
        UpdateLayout();
    }

    /// Clear
    public void Clear()
    {
        try
        {
            FilterMode = DefaultFilterMode;
            Value1 = DefaultValue1;
            Value2 = DefaultValue2;
            UpdateLayout();
        }
        catch { }
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
    private void BtnSymbol_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement c)
            c.ContextMenu.IsOpen = true;
    }

    /// Filter mode change
    private void MnuItmFilterMode_Click(object sender, RoutedEventArgs e)
    {
        FilterMode = (Modes)Enum.Parse(typeof(Modes), ((FrameworkElement)sender).Tag.ToString());

        var symbolButton = (ButtonBase)GetTemplateChild("BtnSymbol");
        var symbolStackPanel = (StackPanel)symbolButton.Content;
        var newSymbolStackPanel = (StackPanel)symbolButton.ContextMenu.Items.OfType<MenuItem>().ToList()[(int)FilterMode].Icon;

        ((OutlinedTextBlock)symbolStackPanel.Children[0]).Fill = ((OutlinedTextBlock)newSymbolStackPanel.Children[0]).Fill;
        ((OutlinedTextBlock)symbolStackPanel.Children[0]).Text = ((OutlinedTextBlock)newSymbolStackPanel.Children[0]).Text;

        ((OutlinedTextBlock)symbolStackPanel.Children[1]).Fill = ((OutlinedTextBlock)newSymbolStackPanel.Children[1]).Fill;
        ((OutlinedTextBlock)symbolStackPanel.Children[1]).Text = ((OutlinedTextBlock)newSymbolStackPanel.Children[1]).Text;

        ((OutlinedTextBlock)symbolStackPanel.Children[2]).Fill = ((OutlinedTextBlock)newSymbolStackPanel.Children[2]).Fill;
        ((OutlinedTextBlock)symbolStackPanel.Children[2]).Text = ((OutlinedTextBlock)newSymbolStackPanel.Children[2]).Text;
    }
}

public class StswColumnFilterData
{
    public string? SqlParam { get; internal set; }
    public string? SqlString { get; internal set; }
    public object? Value1 { get; internal set; }
    public object? Value2 { get; internal set; }
    public object? DefaultValue1 { get; internal set; }
    public object? DefaultValue2 { get; internal set; }
    public Action? Clear { get; internal set; }
}
