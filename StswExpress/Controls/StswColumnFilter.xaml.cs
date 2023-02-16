using System;
using System.Collections;
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
}

public class StswColumnFilterBase : UserControl
{
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
    public enum Types
    {
        Check,
        Date,
        Number,
        Text,
        ListOfNumbers,
        ListOfTexts
    }

    protected ButtonBase? partSymbol;
    protected UniformGrid? partControls;

    #region Style
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion

    #region Properties
    /// BindingData
    public static readonly DependencyProperty BindingDataProperty
        = DependencyProperty.Register(
            nameof(BindingData),
            typeof(StswColumnFilterBindingData),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(StswColumnFilterBindingData?))
        );
    public StswColumnFilterBindingData? BindingData
    {
        get => (StswColumnFilterBindingData?)GetValue(BindingDataProperty);
        set => SetValue(BindingDataProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    
    /// DisplayMemberPath
    public static readonly DependencyProperty DisplayMemberPathProperty
        = DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(StswColumnFilterBase),
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

    /// FilterPadding
    public static readonly DependencyProperty FilterPaddingProperty
        = DependencyProperty.Register(
            nameof(FilterPadding),
            typeof(Thickness),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness FilterPadding
    {
        get => (Thickness)GetValue(FilterPaddingProperty);
        set => SetValue(FilterPaddingProperty, value);
    }

    /// FilterSqlColumn
    public static readonly DependencyProperty FilterSqlColumnProperty
        = DependencyProperty.Register(
            nameof(FilterSqlColumn),
            typeof(string),
            typeof(StswColumnFilterBase),
            new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                FilterSqlColumnChanged, null, false, UpdateSourceTrigger.PropertyChanged)
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
            typeof(StswColumnFilterBase),
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
            typeof(StswColumnFilterBase),
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
            typeof(object),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(object?))
        );
    public object? Header
    {
        get => (object?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// IsFilterCaseSensitive
    public static readonly DependencyProperty IsFilterCaseSensitiveProperty
        = DependencyProperty.Register(
            nameof(IsFilterCaseSensitive),
            typeof(bool),
            typeof(StswColumnFilterBase),
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
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(bool))
        );
    public bool IsFilterNullSensitive
    {
        get => (bool)GetValue(IsFilterNullSensitiveProperty);
        set => SetValue(IsFilterNullSensitiveProperty, value);
    }

    /// ItemsSource
    public static readonly DependencyProperty ItemsSourceProperty
        = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList),
            typeof(StswColumnFilterBase),
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
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(string))
        );
    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    /// SqlParam
    public static readonly DependencyProperty SqlParamProperty
        = DependencyProperty.Register(
            nameof(SqlParam),
            typeof(string),
            typeof(StswColumnFilterBase),
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
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(string?))
        );
    public string? SqlString
    {
        get => (string?)GetValue(SqlStringProperty);
        private set => SetValue(SqlStringProperty, value);
    }

    /// Value1
    public static readonly DependencyProperty Value1Property
        = DependencyProperty.Register(
            nameof(Value1),
            typeof(object),
            typeof(StswColumnFilterBase),
            new FrameworkPropertyMetadata(default(object?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                ValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public object? Value1
    {
        get => (object?)GetValue(Value1Property);
        set => SetValue(Value1Property, value);
    }
    internal object? DefaultValue1 { get; set; } = null;

    /// Value2
    public static readonly DependencyProperty Value2Property
        = DependencyProperty.Register(
            nameof(Value2),
            typeof(object),
            typeof(StswColumnFilterBase),
            new FrameworkPropertyMetadata(default(object?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                ValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public object? Value2
    {
        get => (object?)GetValue(Value2Property);
        set => SetValue(Value2Property, value);
    }
    internal object? DefaultValue2 { get; set; } = null;
    #endregion

    /// SetData
    private void SetData()
    {
        BindingData = new StswColumnFilterBindingData()
        {
            SqlParam = SqlParam,
            SqlString = SqlString,
            Value1 = Value1,
            Value2 = Value2,
            DefaultValue1 = DefaultValue1,
            DefaultValue2 = DefaultValue2,
            Clear = new Action(ClearData),
        };
    }

    /// GenerateSqlString
    public void GenerateSqlString()
    {
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
        SqlString = FilterMode switch
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

    /// ClearData
    public void ClearData()
    {
        var ungFilters = (UniformGrid)GetTemplateChild("PART_Controls");

        FilterMode = DefaultFilterMode;

        if (Value1 is IList list1)
        {
            list1.Clear();
            if (DefaultValue1 != null)
                foreach (var elem in (IList)DefaultValue1)
                    list1.Add(elem);
            if (ungFilters.Children.Count > 0)
                ((StswComboBox)ungFilters.Children[0]).SetText();
            ValueChanged(this, new DependencyPropertyChangedEventArgs());
        }
        else
            Value1 = DefaultValue1;

        if (Value2 is IList list2)
        {
            list2.Clear();
            if (DefaultValue2 != null)
                foreach (var elem in (IList)DefaultValue2)
                    list2.Add(elem);
            if (ungFilters.Children.Count > 1)
                ((StswComboBox)ungFilters.Children[1]).SetText();
            ValueChanged(this, new DependencyPropertyChangedEventArgs());
        }
        else
            Value2 = DefaultValue2;

        var btnSymbol = (ButtonBase)GetTemplateChild("PART_Symbol");
        var btnSymbolItems = btnSymbol.ContextMenu.Items.OfType<MenuItem>().ToList();
        MnuItmFilterMode_Click(btnSymbolItems[(int)(FilterMode ?? 0)], null);
        SetData();
        UpdateLayout();
    }

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        partSymbol = (ButtonBase)GetTemplateChild("PART_Symbol");
        partControls = (UniformGrid)GetTemplateChild("PART_Controls");

        /// set bindings for Value1 and Value2
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
            Path = new PropertyPath(nameof(SubBorderThickness)),
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StswColumnFilterBase), 1),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        var inputbinding = new KeyBinding()
        {
            Command = StswGlobalCommands.Refresh,
            Key = Key.Return
        };

        /// create control based on FilterType
        switch (FilterType)
        {
            case Types.Check:
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
                    partControls.Children.Add(cont1);
                    break;
                }
            
            case Types.Date:
                {
                    var cont1 = new StswDatePicker()
                    {
                        BorderThickness = new Thickness(0),
                        CornerRadius = new CornerRadius(0),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    cont1.InputBindings.Add(inputbinding);
                    cont1.SetBinding(StswDatePickerBase.SelectedDateProperty, binding1);
                    cont1.SetBinding(StswDatePickerBase.SubBorderThicknessProperty, subborderThickness);
                    partControls.Children.Add(cont1);

                    var cont2 = new StswDatePicker()
                    {
                        //BorderThickness = new Thickness(0),
                        CornerRadius = new CornerRadius(0),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    cont2.BorderThickness = new Thickness(0, cont2.BorderThickness.Top, 0, 0);
                    cont2.InputBindings.Add(inputbinding);
                    cont2.SetBinding(StswDatePickerBase.SelectedDateProperty, binding2);
                    cont1.SetBinding(StswDatePickerBase.SubBorderThicknessProperty, subborderThickness);
                    partControls.Children.Add(cont2);
                    break;
                }
                
            case Types.Number:
                {
                    var cont1 = new StswNumericBox()
                    {
                        BorderThickness = new Thickness(0),
                        CornerRadius = new CornerRadius(0),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    cont1.InputBindings.Add(inputbinding);
                    cont1.SetBinding(StswNumericBoxBase.ValueProperty, binding1);
                    cont1.SetBinding(StswNumericBoxBase.SubBorderThicknessProperty, subborderThickness);
                    //cont1.SetBinding(StswComboBox.MinWidthProperty, bindingMinWidth);
                    partControls.Children.Add(cont1);

                    var cont2 = new StswNumericBox()
                    {
                        //BorderThickness = new Thickness(0),
                        CornerRadius = new CornerRadius(0),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    cont2.BorderThickness = new Thickness(0, cont2.BorderThickness.Top, 0, 0);
                    cont2.InputBindings.Add(inputbinding);
                    cont2.SetBinding(StswNumericBoxBase.ValueProperty, binding2);
                    cont1.SetBinding(StswNumericBoxBase.SubBorderThicknessProperty, subborderThickness);
                    //cont2.SetBinding(StswComboBox.MinWidthProperty, bindingMinWidth);
                    partControls.Children.Add(cont2);
                    break;
                }
                
            case Types.Text:
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
                    partControls.Children.Add(cont1);
                    break;
                }

            default:
                if (FilterType.In(Types.ListOfNumbers, Types.ListOfTexts))
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
                    cont1.SetBinding(StswComboBoxBase.SubBorderThicknessProperty, subborderThickness);
                    //cont1.SetBinding(StswComboBox.MinWidthProperty, bindingMinWidth);
                    partControls.Children.Add(cont1);
                }
                
                break;
        }

        /// visibility for FilterMode items
        var btnSymbolItems = partSymbol.ContextMenu.Items.OfType<MenuItem>().ToList();
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
                item.Header = $"_{keynumb++} {item.Header}";

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
        FilterModeChanged(this, new DependencyPropertyChangedEventArgs());

        /// set default values
        DefaultFilterMode = FilterMode;
        DefaultValue1 = FilterType.ToString().StartsWith("List") && Value1 == null ? new List<object>() : Value1;
        DefaultValue2 = FilterType.ToString().StartsWith("List") && Value2 == null ? new List<object>() : Value2;

        ValueChanged(this, new DependencyPropertyChangedEventArgs());
        MnuItmFilterMode_Click(btnSymbolItems[(int)(FilterMode ?? 0)], null);

        base.OnApplyTemplate();
        UpdateLayout();
    }

    #region Callbacks
    /// FilterModeChanged
    public static void FilterModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColumnFilter filter)
        {
            if (filter.partControls?.Children?.Count >= 2)
                filter.partControls.Children[1].Visibility = filter.FilterMode == Modes.Between ? Visibility.Visible : Visibility.Collapsed;
            if (filter.partControls?.Children?.Count >= 1)
                filter.partControls.Children[0].Visibility = !filter.FilterMode.In(Modes.Null, Modes.NotNull) ? Visibility.Visible : Visibility.Hidden;

            if (filter.IsLoaded)
            {
                filter.GenerateSqlString();
                filter.SetData();
            }
        }
    }

    /// FilterSqlColumnChanged
    public static void FilterSqlColumnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColumnFilterBase filter)
        {
            filter.SqlParam = "@" + new string(((string)e.NewValue).Where(char.IsLetterOrDigit).ToArray());
            filter.SetData();
        }
    }

    /// ValueChanged
    public static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColumnFilterBase filter)
        {
            if (filter.FilterType.In(Types.ListOfNumbers, Types.ListOfTexts))
            {
                /*
                if (panel?.Children?.Count > 0)
                    ((StswComboBox)panel.Children[0]).SelectedItems = (IList)e.NewValue;
                else*/
                if (filter.partControls?.Children?.Count == null)
                    filter.Value1 = new List<object>();
            }

            if (filter.Value1 == null || (filter.Value1 is IList l && l.Count == 0)
            || (filter.Value2 == null && filter.FilterMode == Modes.Between))
                filter.SqlString = null;
            else
                filter.GenerateSqlString();

            filter.SetData();
        }
    }
    #endregion

    #region Events
    /// Refresh
    [Obsolete]
    protected void CmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            StswGlobalCommands.Refresh.Execute(null, Parent as UIElement);
        }
        catch { }
    }

    /// FilterMode click
    protected void PART_Symbol_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement c)
            c.ContextMenu.IsOpen = true;
    }

    /// FilterMode change
    protected void MnuItmFilterMode_Click(object sender, RoutedEventArgs e)
    {
        FilterMode = (Modes)Enum.Parse(typeof(Modes), (sender as FrameworkElement)?.Tag?.ToString() ?? string.Empty);

        var symbolText = partSymbol?.Content as OutlinedTextBlock;
        var newSymbolText = partSymbol?.ContextMenu?.Items?.OfType<MenuItem>()?.ToList()?[(int)FilterMode]?.Icon as OutlinedTextBlock;

        if (symbolText != null && newSymbolText != null)
        {
            symbolText.Fill = newSymbolText.Fill;
            symbolText.Text = newSymbolText.Text;
        }
    }
    #endregion
}

public class StswColumnFilterBindingData
{
    public string? SqlParam { get; internal set; }
    public string? SqlString { get; internal set; }
    public object? Value1 { get; internal set; }
    public object? Value2 { get; internal set; }
    public object? DefaultValue1 { get; internal set; }
    public object? DefaultValue2 { get; internal set; }
    public Action? Clear { get; internal set; }
}
