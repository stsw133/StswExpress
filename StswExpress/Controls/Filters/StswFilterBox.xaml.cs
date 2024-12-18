using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A control used for filtering data in a <see cref="StswDataGrid"/>.
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically bind selected items.
/// </summary>
[ContentProperty(nameof(Header))]
public class StswFilterBox : Control, /*IStswBoxControl,*/ IStswCornerControl
{
    public ICommand SelectModeCommand { get; set; }

    public StswFilterBox()
    {
        SelectModeCommand = new StswCommand<StswFilterMode>(x => FilterMode = x);
    }
    static StswFilterBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFilterBox), new FrameworkPropertyMetadata(typeof(StswFilterBox)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswFilterBox), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private ButtonBase? _filterModeButton;
    private StswDataGrid? _dataGrid;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// IsInDataGrid
        _dataGrid = StswFn.FindVisualAncestor<StswDataGrid>(this);
        IsInDataGrid = _dataGrid != null;

        /// ToggleButton: filter mode
        _filterModeButton = GetTemplateChild("PART_FilterMode") as ButtonBase;

        /// default FilterMode
        FilterMode ??= FilterType switch
        {
            StswAdaptiveType.Check or StswAdaptiveType.Date or StswAdaptiveType.Number => StswFilterMode.Equal,
            StswAdaptiveType.List => StswFilterMode.In,
            StswAdaptiveType.Text => StswFilterMode.Contains,
            _ => null
        };

        /// assign default values
        DefaultFilterMode = FilterMode;
        DefaultValue1 = Value1;
        DefaultValue2 = Value2;

        OnFilterModeChanged(this, new DependencyPropertyChangedEventArgs());
        OnValueChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Generates the SQL string based on the current filter settings.
    /// </summary>
    public void GenerateSqlString()
    {
        if (_dataGrid?.FiltersType != StswDataGridFiltersType.SQL)
        {
            SqlString = null;
            return;
        }

        /// separator
        string s = FilterType is StswAdaptiveType.Date or StswAdaptiveType.List or StswAdaptiveType.Text ? "'" : string.Empty;
        /// case sensitive
        string cs1 = FilterType is StswAdaptiveType.List or StswAdaptiveType.Text && !IsFilterCaseSensitive ? "lower(" : string.Empty;
        string cs2 = cs1.Length > 0 ? ")" : string.Empty;
        /// null sensitive
        string ns1 = !IsFilterNullSensitive ? "coalesce(" : string.Empty;
        string ns2 = !IsFilterNullSensitive ? FilterType switch
        {
            StswAdaptiveType.Check => ", 0)",
            StswAdaptiveType.Date => ", '1900-01-01')",
            StswAdaptiveType.List or StswAdaptiveType.Text => ", '')",
            StswAdaptiveType.Number => ", 0)",
            _ => string.Empty
        } : string.Empty;

        /// calculate SQL string
        var selectedItems = ItemsSource?.OfType<IStswSelectionItem>()?.Where(x => x.IsSelected).ToList();
        var listValues = selectedItems?
            .Select(item => SelectedValuePath != null
                ? item.GetType().GetProperty(SelectedValuePath)?.GetValue(item) ?? item
                : item)
            .Select(value => value?.GetType().IsEnum == true ? value.ConvertTo<int>() : value)
            .ToList() ?? [];

        var listString = string.Join($"{s}{cs2},{cs1}{s}", listValues ?? Enumerable.Empty<object?>());

        /// result
        SqlString = FilterMode switch
        {
            StswFilterMode.Equal => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} = {cs1}{SqlParam}1{cs2}",
            StswFilterMode.NotEqual => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} <> {cs1}{SqlParam}1{cs2}",
            StswFilterMode.Greater => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} > {cs1}{SqlParam}1{cs2}",
            StswFilterMode.GreaterEqual => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} >= {cs1}{SqlParam}1{cs2}",
            StswFilterMode.Less => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} < {cs1}{SqlParam}1{cs2}",
            StswFilterMode.LessEqual => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} <= {cs1}{SqlParam}1{cs2}",
            StswFilterMode.Between => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} between {cs1}{SqlParam}1{cs2} and {cs1}{SqlParam}2{cs2}",
            StswFilterMode.Contains => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} like {cs1}concat('%', {SqlParam}1, '%'){cs2}",
            StswFilterMode.NotContains => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} not like {cs1}concat('%', {SqlParam}1, '%'){cs2}",
            StswFilterMode.Like => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} like {cs1}{SqlParam}1{cs2}",
            StswFilterMode.NotLike => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} not like {cs1}{SqlParam}1{cs2}",
            StswFilterMode.StartsWith => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} like {cs1}concat({SqlParam}1, '%'){cs2}",
            StswFilterMode.EndsWith => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} like {cs1}concat('%', {SqlParam}1){cs2}",
            StswFilterMode.In => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} in ({cs1}{s}{listString}{s}{cs2})",
            StswFilterMode.NotIn => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} not in ({cs1}{s}{listString}{s}{cs2})",
            StswFilterMode.Null => $"{FilterValuePath} is null",
            StswFilterMode.NotNull => $"{FilterValuePath} is not null",
            _ => SqlString
        };
    }

    /// <summary>
    /// Generates the CollectionView's Filter predicate based on the current filter settings.
    /// </summary>
    public Predicate<object>? GenerateFilterPredicate()
    {
        if (Value1 == null || string.IsNullOrEmpty(Value1.ToString()))
            return null;

        /// make list
        var selectedItems = ItemsSource?.OfType<IStswSelectionItem>().Where(x => x.IsSelected).ToList();
        var listValues = selectedItems?
            .Select(item => SelectedValuePath != null
                ? item.GetType().GetProperty(SelectedValuePath)?.GetValue(item) ?? item
                : item)
            .Select(value => value?.GetType().IsEnum == true ? value.ConvertTo<int>() : value)
            .ToList() ?? [];

        /// result
        return item =>
        {
            var value = item.GetPropertyValue(FilterValuePath)?.ToString();
            var val1 = Value1?.ToString()?.ToLowerInvariant();
            if (value == null && !IsFilterNullSensitive)
                value = string.Empty;

            return FilterMode switch
            {
                StswFilterMode.Equal => value == val1,
                StswFilterMode.In => listValues.Contains(value),
                StswFilterMode.NotIn => !listValues.Contains(value),
                _ => true
            };
        };
        /*
        return item =>
        {
            var value = GetValueForFilter(item);
            var v = value?.ToString();
            var val1 = Value1?.ToString();
            var val2 = Value2?.ToString();

            if (!IsFilterCaseSensitive)
            {
                v = v?.ToLower();
                val1 = val1?.ToLower();
                val2 = val2?.ToLower();
            }

            if (!IsFilterNullSensitive && v == null)
            {
                v = FilterType switch
                {
                    StswAdaptiveType.Check => false.ToString(),
                    StswAdaptiveType.Date => new DateTime().ToString(),
                    StswAdaptiveType.List => string.Empty,
                    StswAdaptiveType.Number => 0.ToString(),
                    StswAdaptiveType.Text => string.Empty,
                    _ => string.Empty
                };
            }

            return FilterMode switch
            {
                StswFilterMode.Equal => v == val1,
                StswFilterMode.NotEqual => v != val1,
                StswFilterMode.Greater => decimal.TryParse(v, out var decvG) && decimal.TryParse(val1, out var decval1G) && decvG > decval1G,
                StswFilterMode.GreaterEqual => decimal.TryParse(v, out var decvGE) && decimal.TryParse(val1, out var decval1GE) && decvGE >= decval1GE,
                StswFilterMode.Less => decimal.TryParse(v, out var decvL) && decimal.TryParse(val1, out var decval1L) && decvL < decval1L,
                StswFilterMode.LessEqual => decimal.TryParse(v, out var decvLE) && decimal.TryParse(val1, out var decval1LE) && decvLE <= decval1LE,
                StswFilterMode.Between => decimal.TryParse(v, out var decvB)
                                        && decimal.TryParse(val1, out var decval1B)
                                        && decimal.TryParse(val2, out var decval2B)
                                        && decvB >= decval1B && decvB <= decval2B,
                StswFilterMode.Contains => val1 != null && v?.Contains(val1) == true,
                StswFilterMode.NotContains => val1 != null && !(v?.Contains(val1) == true),
                StswFilterMode.StartsWith => val1 != null && v?.StartsWith(val1) == true,
                StswFilterMode.EndsWith => val1 != null && v?.EndsWith(val1) == true,
                StswFilterMode.In => listValues.Any(lv => lv?.ToString()?.Equals(v) == true),
                StswFilterMode.NotIn => !listValues.Any(lv => lv?.ToString()?.Equals(v) == true),
                StswFilterMode.Null => value == null,
                StswFilterMode.NotNull => value != null,
                _ => true,
            };
        };
        */
    }

    /// <summary>
    /// Event handler for handling the KeyDown event.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Enter)
            _dataGrid?.RefreshCommand?.Execute(null);
    }

    public static readonly RoutedEvent FilterChangedEvent
        = EventManager.RegisterRoutedEvent(
            nameof(FilterChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StswFilterBox)
        );
    public event RoutedEventHandler FilterChanged
    {
        add => AddHandler(FilterChangedEvent, value);
        remove => RemoveHandler(FilterChangedEvent, value);
    }
    private void NotifyFilterChanged()
    {
        RaiseEvent(new RoutedEventArgs(FilterChangedEvent));
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the path to the display string property of the items in the ItemsSource (for <see cref="StswSelectionBox"/>).
    /// </summary>
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }
    public static readonly DependencyProperty DisplayMemberPathProperty
        = DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets the menu mode for the filter mode button.
    /// </summary>
    public StswMenuMode FilterMenuMode
    {
        get => (StswMenuMode)GetValue(FilterMenuModeProperty);
        set => SetValue(FilterMenuModeProperty, value);
    }
    public static readonly DependencyProperty FilterMenuModeProperty
        = DependencyProperty.Register(
            nameof(FilterMenuMode),
            typeof(StswMenuMode),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets the current filter mode.
    /// </summary>
    public StswFilterMode? FilterMode
    {
        get => (StswFilterMode?)GetValue(FilterModeProperty);
        set => SetValue(FilterModeProperty, value);
    }
    public static readonly DependencyProperty FilterModeProperty
        = DependencyProperty.Register(
            nameof(FilterMode),
            typeof(StswFilterMode?),
            typeof(StswFilterBox),
            new FrameworkPropertyMetadata(default(StswFilterMode?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFilterModeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnFilterModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilterBox stsw)
        {
            /// visual change for mode box
            if (stsw.FilterMode != null
             && stsw._filterModeButton?.Content is StswOutlinedText symbolBlock
             && stsw._filterModeButton?.ContextMenu?.Items?.OfType<StswMenuItem>()?.First(x => (StswFilterMode?)x.CommandParameter == stsw.FilterMode)?.Icon is StswOutlinedText newSymbolBlock)
            {
                symbolBlock.Fill = newSymbolBlock.Fill;
                symbolBlock.Text = newSymbolBlock.Text;
                symbolBlock.UpdateLayout();
            }
            OnValueChanged(stsw, new DependencyPropertyChangedEventArgs());
        }
    }
    internal StswFilterMode? DefaultFilterMode { get; set; } = null;

    /// <summary>
    /// Gets or sets the type of filter to be applied.
    /// </summary>
    public StswAdaptiveType FilterType
    {
        get => (StswAdaptiveType)GetValue(FilterTypeProperty);
        set => SetValue(FilterTypeProperty, value);
    }
    public static readonly DependencyProperty FilterTypeProperty
        = DependencyProperty.Register(
            nameof(FilterType),
            typeof(StswAdaptiveType),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets either the SQL column name or model's property name used for filtering.
    /// </summary>
    public string FilterValuePath
    {
        get => (string)GetValue(FilterValuePathProperty);
        set => SetValue(FilterValuePathProperty, value);
    }
    public static readonly DependencyProperty FilterValuePathProperty
        = DependencyProperty.Register(
            nameof(FilterValuePath),
            typeof(string),
            typeof(StswFilterBox),
            new PropertyMetadata(default(string), OnFilterValuePathChanged)
        );
    public static void OnFilterValuePathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilterBox stsw)
        {
            stsw.SqlParam = "@" + new string(((string)e.NewValue).Where(char.IsLetterOrDigit).ToArray());
            OnValueChanged(stsw, new DependencyPropertyChangedEventArgs());
        }
    }

    /// <summary>
    /// Gets or sets the visibility of the filter part.
    /// </summary>
    public Visibility FilterVisibility
    {
        get => (Visibility)GetValue(FilterVisibilityProperty);
        set => SetValue(FilterVisibilityProperty, value);
    }
    public static readonly DependencyProperty FilterVisibilityProperty
        = DependencyProperty.Register(
            nameof(FilterVisibility),
            typeof(Visibility),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets the custom format string used to display the value in the control.
    /// When set, the value is formatted according to the provided format string.
    /// </summary>
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static readonly DependencyProperty FormatProperty
        = DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(StswFilterBox),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

    /// <summary>
    /// Gets or sets the header of the control.
    /// </summary>
    public object? Header
    {
        get => (object?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether or not the drop-down portion of the control is currently open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the filter is case-sensitive.
    /// </summary>
    public bool IsFilterCaseSensitive
    {
        get => (bool)GetValue(IsFilterCaseSensitiveProperty);
        set => SetValue(IsFilterCaseSensitiveProperty, value);
    }
    public static readonly DependencyProperty IsFilterCaseSensitiveProperty
        = DependencyProperty.Register(
            nameof(IsFilterCaseSensitive),
            typeof(bool),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the filter is null-sensitive.
    /// </summary>
    public bool IsFilterNullSensitive
    {
        get => (bool)GetValue(IsFilterNullSensitiveProperty);
        set => SetValue(IsFilterNullSensitiveProperty, value);
    }
    public static readonly DependencyProperty IsFilterNullSensitiveProperty
        = DependencyProperty.Register(
            nameof(IsFilterNullSensitive),
            typeof(bool),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// 
    /// </summary>
    internal bool IsInDataGrid
    {
        get => (bool)GetValue(IsInDataGridProperty);
        set => SetValue(IsInDataGridProperty, value);
    }
    internal static readonly DependencyProperty IsInDataGridProperty
        = DependencyProperty.Register(
            nameof(IsInDataGrid),
            typeof(bool),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets the collection that is used to generate the content of the StswSelectionBox.
    /// </summary>
    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public static readonly DependencyProperty ItemsSourceProperty
        = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList),
            typeof(StswFilterBox),
            new FrameworkPropertyMetadata(default,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsSourceChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilterBox stsw)
        {
            if (e.NewValue?.GetType()?.IsListType(out var innerType) == true)
            {
                if (innerType?.IsAssignableTo(typeof(IStswSelectionItem)) != true)
                    throw new Exception($"{nameof(ItemsSource)} of {nameof(StswFilterBox)} has to implement {nameof(IStswSelectionItem)} interface!");

                /// StswComboItem short usage
                if (innerType?.IsAssignableTo(typeof(StswComboItem)) == true)
                {
                    if (string.IsNullOrEmpty(stsw.DisplayMemberPath))
                        stsw.DisplayMemberPath = nameof(StswComboItem.Display);
                    if (string.IsNullOrEmpty(stsw.SelectedValuePath))
                        stsw.SelectedValuePath = nameof(StswComboItem.Value);
                }
            }
        }
    }
    internal IList? DefaultItemsSource { get; set; } = null;

    /// <summary>
    /// Gets or sets the path to the value property of the selected items in the ItemsSource (for <see cref="StswSelectionBox"/>).
    /// </summary>
    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }
    public static readonly DependencyProperty SelectedValuePathProperty
        = DependencyProperty.Register(
            nameof(SelectedValuePath),
            typeof(string),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets the selection unit of the control.
    /// </summary>
    public StswCalendarUnit SelectionUnit
    {
        get => (StswCalendarUnit)GetValue(SelectionUnitProperty);
        set => SetValue(SelectionUnitProperty, value);
    }
    public static readonly DependencyProperty SelectionUnitProperty
        = DependencyProperty.Register(
            nameof(SelectionUnit),
            typeof(StswCalendarUnit),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets the SQL parameter used in the SQL string generation.
    /// </summary>
    public string SqlParam
    {
        get => (string)GetValue(SqlParamProperty);
        private set => SetValue(SqlParamProperty, value);
    }
    public static readonly DependencyProperty SqlParamProperty
        = DependencyProperty.Register(
            nameof(SqlParam),
            typeof(string),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets the generated SQL string used for filtering data.
    /// </summary>
    public string? SqlString
    {
        get => (string?)GetValue(SqlStringProperty);
        private set => SetValue(SqlStringProperty, value);
    }
    public static readonly DependencyProperty SqlStringProperty
        = DependencyProperty.Register(
            nameof(SqlString),
            typeof(string),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets the first value used in filtering.
    /// </summary>
    public object? Value1
    {
        get => (object?)GetValue(Value1Property);
        set => SetValue(Value1Property, value);
    }
    public static readonly DependencyProperty Value1Property
        = DependencyProperty.Register(
            nameof(Value1),
            typeof(object),
            typeof(StswFilterBox),
            new FrameworkPropertyMetadata(default,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilterBox stsw)
        {
           //if (stsw.Value1 == null
           //|| (stsw.Value2 == null && stsw.FilterMode == StswFilterMode.Between)
           //|| stsw.ItemsSource?.OfType<IStswSelectionItem>()?.Where(x => x.IsSelected)?.Count() == 0)
           //    stsw.SqlString = null;
           //else
           //    stsw.GenerateSqlString();

            if (stsw._dataGrid?.FiltersType == StswDataGridFiltersType.SQL)
            {
                if (stsw.Value1 == null
            || (stsw.Value2 == null && stsw.FilterMode == StswFilterMode.Between)
            || stsw.ItemsSource?.OfType<IStswSelectionItem>()?.Where(x => x.IsSelected)?.Count() == 0)
                    stsw.SqlString = null;
                else
                    stsw.GenerateSqlString();
            }
            else
            {
                stsw._dataGrid?.RegisterExternalFilter(stsw, stsw.GenerateFilterPredicate());
            }

            stsw.NotifyFilterChanged();
        }
    }
    internal object? DefaultValue1 { get; set; } = null;

    /// <summary>
    /// Gets or sets the second value used in filtering (e.g., for range filters).
    /// </summary>
    public object? Value2
    {
        get => (object?)GetValue(Value2Property);
        set => SetValue(Value2Property, value);
    }
    public static readonly DependencyProperty Value2Property
        = DependencyProperty.Register(
            nameof(Value2),
            typeof(object),
            typeof(StswFilterBox),
            new FrameworkPropertyMetadata(default,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    internal object? DefaultValue2 { get; set; } = null;
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswFilterBox),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswFilterBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between box and drop-down button.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswFilterBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/// <summary>
/// 
/// </summary>
internal class StswFilterAggregator
{
    private readonly Dictionary<object, Predicate<object>> _registeredFilters = [];

    /// <summary>
    /// 
    /// </summary>
    public void RegisterFilter(object key, Predicate<object>? filter)
    {
        if (filter == null)
            _registeredFilters.Remove(key);
        else
            _registeredFilters[key] = filter;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CombinedFilter(object item)
    {
        if (_registeredFilters.Count == 0)
            return true;

        foreach (var filter in _registeredFilters.Values)
        {
            if (filter == null)
                continue;

            if (!filter(item))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool HasFilters => _registeredFilters.Count > 0;
}
