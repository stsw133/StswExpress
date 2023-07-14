using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

[ContentProperty(nameof(Header))]
public class StswFilter : UserControl
{
    public ICommand SelectModeCommand { get; set; }

    public StswFilter()
    {
        SelectModeCommand = new StswRelayCommand<object?>(SelectMode_Executed);
    }
    static StswFilter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFilter), new FrameworkPropertyMetadata(typeof(StswFilter)));
    }

    #region Events
    private ButtonBase? partFilterMode;
    private StswDataGrid? stswDataGrid;

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// 
        if (StswFn.FindVisualAncestor<StswDataGrid>(this) is StswDataGrid dataGrid)
            stswDataGrid = dataGrid;

        /// ToggleButton: filter mode
        if (GetTemplateChild("PART_FilterMode") is ButtonBase btnMode)
            partFilterMode = btnMode;

        /// Refresh on enter
        KeyDown += OnKeyDown;
        /*
        /// shortcuts for FilterMode items
        var keynumb = 1;
        foreach (var item in partFilterMode?.ContextMenu?.Items?.OfType<MenuItem>()?.ToList()?.Where(x => x.Visibility == Visibility.Visible))
            if (!char.IsNumber(((string)item.Header)[2]))
                item.Header = $"_{keynumb++} {item.Header}";
        */
        /// default FilterMode
        if (FilterMode == null)
        {
            FilterMode = FilterType switch
            {
                Types.Check => Modes.Equal,
                Types.Date => Modes.Equal,
                Types.List => Modes.In,
                Types.Number => Modes.Equal,
                Types.Text => Modes.Contains,
                _ => null
            };
        }
        else OnFilterModeChanged(this, new DependencyPropertyChangedEventArgs());

        /// assign default values
        DefaultFilterMode = FilterMode;
        DefaultValue1 = Value1;
        DefaultValue2 = Value2;
        //if (SelectedItemsBinding != null)
        //    DefaultSelectedItemsBinding = SelectedItemsBinding.Clone();

        OnValueChanged(this, new DependencyPropertyChangedEventArgs());

        base.OnApplyTemplate();
    }

    /// Command: select mode
    protected void SelectMode_Executed(object? parameter)
    {
        if (parameter is MenuItem f && f.Tag is Modes m)
            FilterMode = m;
    }

    /// GenerateSqlString
    public void GenerateSqlString()
    {
        /// separator
        var s = FilterType.In(Types.Date, Types.List, Types.Text) ? "'" : string.Empty;
        /// case sensitive
        var cs1 = FilterType.In(Types.List, Types.Text) && !IsFilterCaseSensitive ? "lower(" : string.Empty;
        var cs2 = FilterType.In(Types.List, Types.Text) && !IsFilterCaseSensitive ? ")" : string.Empty;
        /// null sensitive
        var ns1 = !IsFilterNullSensitive ? "coalesce(" : string.Empty;
        var ns2 = string.Empty;
        if (!IsFilterNullSensitive)
        {
            ns2 = FilterType switch
            {
                Types.Check => ", 0)",
                Types.Date => ", '1900-01-01')",
                Types.List => ", '')",
                Types.Number => ", 0)",
                Types.Text => ", '')",
                _ => string.Empty
            };
        }

        /// calculate SQL string
        var listValues = new List<object?>();
        if (SelectedItemsBinding != null)
            foreach (var selectedItem in SelectedItemsBinding)
            {
                if (SelectedValuePath != null && selectedItem.GetType().GetProperty(SelectedValuePath) is PropertyInfo propertyInfo)
                    listValues.Add(propertyInfo.GetValue(selectedItem));
                else
                    listValues.Add(selectedItem);
            }

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
            Modes.In => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", listValues)}{s}{cs2})",
            Modes.NotIn => $"{cs1}{ns1}{FilterSqlColumn}{ns2}{cs2} not in ({cs1}{s}{string.Join($"{s}{cs2},{cs1}{s}", listValues)}{s}{cs2})",
            Modes.Null => $"{FilterSqlColumn} is null)",
            Modes.NotNull => $"{FilterSqlColumn} is not null)",
            _ => null
        };
    }

    /// OnKeyDown
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            stswDataGrid?.RefreshCommand?.Execute(null);
    }
    #endregion

    #region Main properties
    /// DisplayMemberPath
    public static readonly DependencyProperty DisplayMemberPathProperty
        = DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(StswFilter)
        );
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// FilterMode
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
    public static readonly DependencyProperty FilterModeProperty
        = DependencyProperty.Register(
            nameof(FilterMode),
            typeof(Modes?),
            typeof(StswFilter),
            new FrameworkPropertyMetadata(default(Modes?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFilterModeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public Modes? FilterMode
    {
        get => (Modes?)GetValue(FilterModeProperty);
        set => SetValue(FilterModeProperty, value);
    }
    public static void OnFilterModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilter stsw)
        {
            if (stsw.FilterMode != null
             && stsw.partFilterMode?.Content is StswOutlinedText symbolBlock and not null
             && stsw.partFilterMode?.ContextMenu?.Items?.OfType<MenuItem>()?.ToList()?[(int)stsw.FilterMode]?.Icon is StswOutlinedText newSymbolBlock and not null)
            {
                symbolBlock.Fill = newSymbolBlock.Fill;
                symbolBlock.Text = newSymbolBlock.Text;
                symbolBlock.UpdateLayout();
            }
            OnValueChanged(stsw, new DependencyPropertyChangedEventArgs());
        }
    }
    internal Modes? DefaultFilterMode { get; set; } = null;

    /// FilterSqlColumn
    public static readonly DependencyProperty FilterSqlColumnProperty
        = DependencyProperty.Register(
            nameof(FilterSqlColumn),
            typeof(string),
            typeof(StswFilter),
            new PropertyMetadata(default(string), OnFilterSqlColumnChanged)
        );
    public string FilterSqlColumn
    {
        get => (string)GetValue(FilterSqlColumnProperty);
        set => SetValue(FilterSqlColumnProperty, value);
    }
    public static void OnFilterSqlColumnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilter stsw)
        {
            stsw.SqlParam = "@" + new string(((string)e.NewValue).Where(char.IsLetterOrDigit).ToArray());
            OnValueChanged(stsw, new DependencyPropertyChangedEventArgs());
        }
    }

    /// FilterType
    public enum Types
    {
        Check,
        Date,
        List,
        Number,
        Text
    }
    public static readonly DependencyProperty FilterTypeProperty
        = DependencyProperty.Register(
            nameof(FilterType),
            typeof(Types),
            typeof(StswFilter)
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
            typeof(StswFilter)
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
            typeof(StswFilter)
        );
    public object? Header
    {
        get => (object?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// IsDropDownOpen
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswFilter)
        );
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// IsFilterCaseSensitive
    public static readonly DependencyProperty IsFilterCaseSensitiveProperty
        = DependencyProperty.Register(
            nameof(IsFilterCaseSensitive),
            typeof(bool),
            typeof(StswFilter)
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
            typeof(StswFilter)
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
            typeof(StswFilter),
            new PropertyMetadata(default, OnItemsSourceChanged)
        );
    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilter stsw)
        {
            if (stsw.SelectedItemsBinding != null)
                stsw.DefaultSelectedItemsBinding = stsw.SelectedItemsBinding.Clone();
            else if (stsw.ItemsSource != null)
                stsw.DefaultSelectedItemsBinding = (IList)Activator.CreateInstance(stsw.ItemsSource.GetType());

            if (stsw.SelectedItemsBinding == null && stsw.DefaultSelectedItemsBinding != null)
                stsw.SelectedItemsBinding = stsw.DefaultSelectedItemsBinding.Clone();
        }
    }

    /// SelectedItemsBinding
    public static readonly DependencyProperty SelectedItemsBindingProperty
        = DependencyProperty.Register(
            nameof(SelectedItemsBinding),
            typeof(IList),
            typeof(StswFilter),
            new FrameworkPropertyMetadata(default(IList),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public IList SelectedItemsBinding
    {
        get => (IList)GetValue(SelectedItemsBindingProperty);
        set => SetValue(SelectedItemsBindingProperty, value);
    }
    internal IList? DefaultSelectedItemsBinding { get; set; } = null;

    /// SelectedValuePath
    public static readonly DependencyProperty SelectedValuePathProperty
        = DependencyProperty.Register(
            nameof(SelectedValuePath),
            typeof(string),
            typeof(StswFilter)
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
            typeof(StswFilter)
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
            typeof(StswFilter)
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
            typeof(StswFilter),
            new FrameworkPropertyMetadata(default,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public object? Value1
    {
        get => (object?)GetValue(Value1Property);
        set => SetValue(Value1Property, value);
    }
    public static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilter stsw)
        {
            if (stsw.Value1 == null || stsw.Value2 == null && stsw.FilterMode == Modes.Between)
                stsw.SqlString = null;
            else
                stsw.GenerateSqlString();
        }
    }
    internal object? DefaultValue1 { get; set; } = null;

    /// Value2
    public static readonly DependencyProperty Value2Property
        = DependencyProperty.Register(
            nameof(Value2),
            typeof(object),
            typeof(StswFilter),
            new FrameworkPropertyMetadata(default,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public object? Value2
    {
        get => (object?)GetValue(Value2Property);
        set => SetValue(Value2Property, value);
    }
    internal object? DefaultValue2 { get; set; } = null;
    #endregion

    #region Spatial properties
    /// > SubBorder ...
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswFilter)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswFilter)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
