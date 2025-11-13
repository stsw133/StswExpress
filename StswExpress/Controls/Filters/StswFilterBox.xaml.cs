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
/// A filtering control designed for use with <see cref="StswDataGrid"/>.
/// Supports multiple filter modes, SQL query generation, and case-sensitive or null-sensitive filtering.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswFilterBox Header="Name" FilterType="Text" FilterMode="Contains" FilterValuePath="Name"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Header))]
public class StswFilterBox : Control, IStswCornerControl
{
    private ButtonBase? _filterModeButton;
    private StswDataGrid? _dataGrid;
    public ICommand SelectModeCommand { get; }

    public StswFilterBox()
    {
        SelectModeCommand = new StswCommand<StswFilterMode>(x => FilterMode = x);
    }
    static StswFilterBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFilterBox), new FrameworkPropertyMetadata(typeof(StswFilterBox)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// find if the control is placed in StswDataGrid
        _dataGrid = StswFnUI.FindVisualAncestor<StswDataGrid>(this);
        IsInDataGrid = _dataGrid != null;

        /// ToggleButton: filter mode
        _filterModeButton = GetTemplateChild("PART_FilterMode") as ButtonBase;

        /// default FilterType
        if (FilterType == StswAdaptiveType.Auto)
        {
            var inferredType = DetermineFilterTypeFromColumn();
            if (inferredType == StswAdaptiveType.Auto)
                inferredType = StswAdaptiveType.Text;
            FilterType = inferredType;
        }

        /// default FilterMode
        FilterMode ??= FilterType switch
        {
            StswAdaptiveType.Check or StswAdaptiveType.Date or StswAdaptiveType.Number or StswAdaptiveType.Time => StswFilterMode.Equal,
            StswAdaptiveType.List => StswFilterMode.In,
            StswAdaptiveType.Text => StswFilterMode.Contains,
            _ => null
        };

        /// assign default values
        DefaultFilterMode = FilterMode;
        DefaultValue1 = Value1;
        DefaultValue2 = Value2;

        /// force first evaluation
        OnFilterModeChanged(this, new DependencyPropertyChangedEventArgs());
        OnValueChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Enter)
            _dataGrid?.RefreshCommand?.Execute(null);
    }

    /// <summary>
    /// Determines the filter type based on the associated DataGrid column type.
    /// </summary>
    /// <returns>The inferred <see cref="StswAdaptiveType"/> for filtering.</returns>
    private StswAdaptiveType DetermineFilterTypeFromColumn()
    {
        var columnHeader = StswFnUI.FindVisualAncestor<DataGridColumnHeader>(this);
        if (columnHeader?.Column is not DataGridColumn column)
            return StswAdaptiveType.Auto;

        return column switch
        {
            StswDataGridTextColumn => StswAdaptiveType.Text,
            StswDataGridDecimalColumn => StswAdaptiveType.Number,
            StswDataGridDoubleColumn => StswAdaptiveType.Number,
            StswDataGridIntegerColumn => StswAdaptiveType.Number,
            StswDataGridDateColumn => StswAdaptiveType.Date,
            StswDataGridCheckColumn => StswAdaptiveType.Check,
            StswDataGridComboColumn => StswAdaptiveType.List,
            StswDataGridTimeColumn => StswAdaptiveType.Time,
            _ => StswAdaptiveType.Auto
        };
    }
    #endregion

    #region ICollectionView filtering
    /// <summary>
    /// Generates a filter predicate for use in a CollectionView filter.
    /// </summary>
    /// <returns>A predicate function that evaluates whether an item should be included.</returns>
    public Predicate<object>? GenerateFilterPredicate()
    {
        if (FilterMode == null)
            return null;

        if (Value1 == null && !FilterMode.In(StswFilterMode.Null, StswFilterMode.NotNull))
            return null;

        /// build selection list if applicable
        var selectedItems = ItemsSource?.OfType<IStswSelectionItem>().Where(x => x.IsSelected).ToList();
        var listValues = selectedItems?
            .Select(item => SelectedValuePath != null
                ? item.GetType().GetProperty(SelectedValuePath)?.GetValue(item) ?? item
                : item)
            .ToList()
            ?? [];

        /// helper method to get value
        object? GetValueForFilter(object rowItem)
        {
            var rawValue = rowItem.GetPropertyValue(FilterValuePath);
            if (rawValue == null && ApplyNullReplacement)
            {
                return FilterType switch
                {
                    StswAdaptiveType.Check => false,
                    StswAdaptiveType.Date => new DateTime(1900, 1, 1),
                    StswAdaptiveType.List => string.Empty,
                    StswAdaptiveType.Number => 0m,
                    StswAdaptiveType.Text => string.Empty,
                    StswAdaptiveType.Time => new TimeSpan(0, 0, 0),
                    _ => string.Empty
                };
            }
            return rawValue;
        }

        /// result
        return item =>
        {
            var rowValue = GetValueForFilter(item);

            if (FilterMode == StswFilterMode.Null)
                return rowValue == null;
            if (FilterMode == StswFilterMode.NotNull)
                return rowValue != null;

            switch (FilterType)
            {
                case StswAdaptiveType.Check:
                    {
                        if (!bool.TryParse(rowValue?.ToString(), out var boolValue))
                            return false;

                        _ = bool.TryParse(Value1?.ToString(), out var boolVal1);

                        return FilterMode switch
                        {
                            StswFilterMode.Equal => boolValue == boolVal1,
                            StswFilterMode.NotEqual => boolValue != boolVal1,
                            _ => true
                        };
                    }
                case StswAdaptiveType.Date:
                    {
                        if (!DateTime.TryParse(rowValue?.ToString(), out var dateValue))
                            return false;

                        _ = DateTime.TryParse(Value1?.ToString(), out var dateVal1);
                        _ = DateTime.TryParse(Value2?.ToString(), out var dateVal2);

                        return FilterMode switch
                        {
                            StswFilterMode.Equal => dateValue.Date == dateVal1.Date,
                            StswFilterMode.NotEqual => dateValue.Date != dateVal1.Date,
                            StswFilterMode.Greater => dateValue.Date > dateVal1.Date,
                            StswFilterMode.GreaterEqual => dateValue.Date >= dateVal1.Date,
                            StswFilterMode.Less => dateValue.Date < dateVal1.Date,
                            StswFilterMode.LessEqual => dateValue.Date <= dateVal1.Date,
                            StswFilterMode.Between => dateValue.Date >= dateVal1.Date && dateValue.Date <= dateVal2.Date,
                            _ => true
                        };
                    }
                case StswAdaptiveType.Number:
                    {
                        if (!decimal.TryParse(rowValue?.ToString(), out var decValue))
                            return false;

                        _ = decimal.TryParse(Value1?.ToString(), out var decVal1);
                        _ = decimal.TryParse(Value2?.ToString(), out var decVal2);

                        return FilterMode switch
                        {
                            StswFilterMode.Equal => decValue == decVal1,
                            StswFilterMode.NotEqual => decValue != decVal1,
                            StswFilterMode.Greater => decValue > decVal1,
                            StswFilterMode.GreaterEqual => decValue >= decVal1,
                            StswFilterMode.Less => decValue < decVal1,
                            StswFilterMode.LessEqual => decValue <= decVal1,
                            StswFilterMode.Between => decValue >= decVal1 && decValue <= decVal2,
                            _ => true
                        };
                    }
                case StswAdaptiveType.Time:
                    {
                        if (!TimeSpan.TryParse(rowValue?.ToString(), out var timeValue))
                            return false;

                        _ = TimeSpan.TryParse(Value1?.ToString(), out var timeVal1);
                        _ = TimeSpan.TryParse(Value2?.ToString(), out var timeVal2);

                        return FilterMode switch
                        {
                            StswFilterMode.Equal => timeValue.TotalSeconds == timeVal1.TotalSeconds,
                            StswFilterMode.NotEqual => timeValue.TotalSeconds != timeVal1.TotalSeconds,
                            StswFilterMode.Greater => timeValue.TotalSeconds > timeVal1.TotalSeconds,
                            StswFilterMode.GreaterEqual => timeValue.TotalSeconds >= timeVal1.TotalSeconds,
                            StswFilterMode.Less => timeValue.TotalSeconds < timeVal1.TotalSeconds,
                            StswFilterMode.LessEqual => timeValue.TotalSeconds <= timeVal1.TotalSeconds,
                            StswFilterMode.Between => timeValue.TotalSeconds >= timeVal1.TotalSeconds && timeValue.TotalSeconds <= timeVal2.TotalSeconds,
                            _ => true
                        };
                    }
                case StswAdaptiveType.List:
                case StswAdaptiveType.Text:
                default:
                    {
                        var textValue = rowValue?.ToString();
                        var val1 = Value1?.ToString();
                        var val2 = Value2?.ToString() ?? string.Empty;

                        if (ApplyCaseTransform)
                        {
                            textValue = textValue?.ToLowerInvariant();
                            val1 = val1?.ToLowerInvariant();
                            val2 = val2?.ToLowerInvariant();
                        }

                        if (textValue == null && !ApplyNullReplacement)
                            return false;

                        if (FilterMode == StswFilterMode.In)
                        {
                            return listValues.Any(o =>
                            {
                                var str = o?.ToString();
                                if (ApplyCaseTransform)
                                    str = str?.ToLowerInvariant();
                                return str == textValue;
                            });
                        }
                        if (FilterMode == StswFilterMode.NotIn)
                        {
                            return !listValues.Any(o =>
                            {
                                var str = o?.ToString();
                                if (ApplyCaseTransform)
                                    str = str?.ToLowerInvariant();
                                return str == textValue;
                            });
                        }

                        return FilterMode switch
                        {
                            StswFilterMode.Equal => textValue == val1,
                            StswFilterMode.NotEqual => textValue != val1,
                            StswFilterMode.Contains => val1 != null && textValue?.Contains(val1) == true,
                            StswFilterMode.NotContains => val1 != null && !(textValue?.Contains(val1) == true),
                            StswFilterMode.StartsWith => val1 != null && textValue?.StartsWith(val1) == true,
                            StswFilterMode.EndsWith => val1 != null && textValue?.EndsWith(val1) == true,
                            StswFilterMode.Like => MatchesLikePattern(textValue, val1),
                            StswFilterMode.NotLike => !MatchesLikePattern(textValue, val1),
                            StswFilterMode.Between => textValue.Between(val1, val2),
                            _ => true
                        };
                    }
            }
        };
    }

    /// <summary>
    /// Interprets SQL-style LIKE patterns (e.g., "%abc%" for contains).
    /// </summary>
    /// <param name="text">The text to compare.</param>
    /// <param name="pattern">The LIKE pattern.</param>
    /// <returns><see langword="true"/> if the text matches the pattern, otherwise <see langword="false"/>.</returns>
    private bool MatchesLikePattern(string? text, string? pattern)
    {
        if (string.IsNullOrEmpty(pattern) || text == null)
            return false;

        var startsWithPercent = pattern.StartsWith('%');
        var endsWithPercent = pattern.EndsWith('%');
        var core = pattern.Trim('%');

        if (startsWithPercent && endsWithPercent)
            return text.Contains(core);
        if (startsWithPercent)
            return text.EndsWith(core);
        if (endsWithPercent)
            return text.StartsWith(core);

        return text == core;
    }
    #endregion

    #region SQL filtering
    /// <summary>
    /// Generates an SQL WHERE clause string based on the current filter settings.
    /// </summary>
    public void GenerateSqlString()
    {
        if (_dataGrid?.FiltersType != StswDataGridFiltersType.SQL)
        {
            SqlString = null;
            return;
        }

        /// check if list values are selected
        var isListFilter = FilterMode is StswFilterMode.In or StswFilterMode.NotIn;
        var valueType = isListFilter ? ResolveListValueType() : null;
        var listIsNumeric = valueType != null && (valueType.IsEnum || valueType.IsNumericType());
        var effType = FilterType == StswAdaptiveType.List
            ? (listIsNumeric ? StswAdaptiveType.Number : StswAdaptiveType.Text)
            : FilterType;

        /// separator
        string s = effType is StswAdaptiveType.Date or StswAdaptiveType.Text or StswAdaptiveType.Time ? "'" : string.Empty;
        /// case sensitive
        string cs1 = effType is StswAdaptiveType.Text && ApplyCaseTransform ? "lower(" : string.Empty;
        string cs2 = cs1.Length > 0 ? ")" : string.Empty;
        /// null sensitive
        string ns1 = ApplyNullReplacement ? "coalesce(" : string.Empty;
        string ns2 = ApplyNullReplacement ? effType switch
        {
            StswAdaptiveType.Check => ", 0)",
            StswAdaptiveType.Date => ", '1900-01-01')",
            StswAdaptiveType.List or StswAdaptiveType.Text => ", '')",
            StswAdaptiveType.Number => ", 0)",
            StswAdaptiveType.Time => ", '00:00:00')",
            _ => string.Empty
        } : string.Empty;

        /// helper method to escape text values
        string EscapeIfText(object? v) =>
            v == null ? string.Empty
              : (s.Length > 0 ? v.ToString()?.Replace("'", "''") ?? string.Empty
                              : v.ToString() ?? string.Empty);

        IEnumerable<string> EnumerateValues()
        {
            if (ItemsSource == null) yield break;
            foreach (var it in ItemsSource.OfType<IStswSelectionItem>())
            {
                if (!it.IsSelected) continue;

                object? v = SelectedValuePath != null
                    ? it.GetType().GetProperty(SelectedValuePath)?.GetValue(it) ?? it
                    : it;

                if (v?.GetType().IsEnum == true)
                    v = Convert.ToInt32(v);

                yield return EscapeIfText(v);
            }
        }

        var listString = string.Join($"{s}{cs2},{cs1}{s}", EnumerateValues());

        /// build final SQL
        SqlString = FilterMode switch
        {
            StswFilterMode.Equal        => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} = {cs1}{SqlParam}1{cs2}",
            StswFilterMode.NotEqual     => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} <> {cs1}{SqlParam}1{cs2}",
            StswFilterMode.Greater      => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} > {cs1}{SqlParam}1{cs2}",
            StswFilterMode.GreaterEqual => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} >= {cs1}{SqlParam}1{cs2}",
            StswFilterMode.Less         => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} < {cs1}{SqlParam}1{cs2}",
            StswFilterMode.LessEqual    => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} <= {cs1}{SqlParam}1{cs2}",
            StswFilterMode.Between      => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} between {cs1}{SqlParam}1{cs2} and {cs1}{SqlParam}2{cs2}",
            StswFilterMode.Contains     => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} like {cs1}concat('%', {SqlParam}1, '%'){cs2}",
            StswFilterMode.NotContains  => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} not like {cs1}concat('%', {SqlParam}1, '%'){cs2}",
            StswFilterMode.Like         => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} like {cs1}{SqlParam}1{cs2}",
            StswFilterMode.NotLike      => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} not like {cs1}{SqlParam}1{cs2}",
            StswFilterMode.StartsWith   => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} like {cs1}concat({SqlParam}1, '%'){cs2}",
            StswFilterMode.EndsWith     => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} like {cs1}concat('%', {SqlParam}1){cs2}",
            StswFilterMode.In           => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} in ({cs1}{s}{listString}{s}{cs2})",
            StswFilterMode.NotIn        => $"{cs1}{ns1}{FilterValuePath}{ns2}{cs2} not in ({cs1}{s}{listString}{s}{cs2})",
            StswFilterMode.Null         => $"{FilterValuePath} is null",
            StswFilterMode.NotNull      => $"{FilterValuePath} is not null",
            _ => SqlString
        };
    }

    /// <summary>
    /// Generates the SQL parameter name for the filter value.
    /// </summary>
    /// <param name="sampleLimit">The maximum number of items to sample for determining the value type.</param>
    /// <returns>The SQL parameter name.</returns>
    private Type? ResolveListValueType(int sampleLimit = 8)
    {
        if (ItemsSource?.GetType().IsListType(out var innerType) == true && innerType != null && !string.IsNullOrEmpty(SelectedValuePath))
        {
            var propType = innerType.GetProperty(SelectedValuePath)?.PropertyType;
            if (propType != null && propType != typeof(object))
                return propType;
        }

        if (ItemsSource == null)
            return null;

        int seen = 0;
        foreach (var it in ItemsSource.OfType<IStswSelectionItem>())
        {
            if (!it.IsSelected)
                continue;

            object? val = SelectedValuePath != null
                ? it.GetType().GetProperty(SelectedValuePath)?.GetValue(it) ?? it
                : it;

            if (val?.GetType().IsEnum == true)
                val = Convert.ToInt32(val);

            if (val != null)
                return val.GetType();

            if (++seen >= sampleLimit)
                break;
        }

        return null;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets whether to apply case transformation to the filter values.
    /// </summary>
    public bool ApplyCaseTransform
    {
        get => (bool)GetValue(ApplyCaseTransformProperty);
        set => SetValue(ApplyCaseTransformProperty, value);
    }
    public static readonly DependencyProperty ApplyCaseTransformProperty
        = DependencyProperty.Register(
            nameof(ApplyCaseTransform),
            typeof(bool),
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets whether to apply a null replacement for the filter values.
    /// </summary>
    public bool ApplyNullReplacement
    {
        get => (bool)GetValue(ApplyNullReplacementProperty);
        set => SetValue(ApplyNullReplacementProperty, value);
    }
    public static readonly DependencyProperty ApplyNullReplacementProperty
        = DependencyProperty.Register(
            nameof(ApplyNullReplacement),
            typeof(bool),
            typeof(StswFilterBox)
        );

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
    /// Raises the <see cref="FilterChanged"/> event to notify that the filter has changed.
    /// </summary>
    //[StswInfo("0.15.0")]
    public event RoutedEventHandler FilterChanged
    {
        add => AddHandler(FilterChangedEvent, value);
        remove => RemoveHandler(FilterChangedEvent, value);
    }
    public static readonly RoutedEvent FilterChangedEvent
        = EventManager.RegisterRoutedEvent(
            nameof(FilterChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
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
    /// Gets or sets the current filtering mode (e.g., Equals, Contains, Between).
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
    public static void OnFilterModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswFilterBox stsw)
            return;

        /// update visual symbol if found
        if (stsw.FilterMode != null
         && stsw._filterModeButton?.Content is StswOutlinedText symbolBlock
         && stsw._filterModeButton?.ContextMenu?.Items?.OfType<StswMenuItem>()?.FirstOrDefault(x => (StswFilterMode?)x.CommandParameter == stsw.FilterMode)?.Icon is StswOutlinedText newSymbolBlock)
        {
            symbolBlock.Fill = newSymbolBlock.Fill;
            symbolBlock.Text = newSymbolBlock.Text;
            symbolBlock.UpdateLayout();
        }
        OnValueChanged(stsw, new DependencyPropertyChangedEventArgs());
    }
    internal StswFilterMode? DefaultFilterMode { get; set; } = null;

    /// <summary>
    /// Gets or sets the data type of the filtered column (Text, Number, Date, etc.).
    /// Determines the appropriate filtering behavior.
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
            typeof(StswFilterBox),
            new FrameworkPropertyMetadata(StswAdaptiveType.Auto)
        );

    /// <summary>
    /// Gets or sets the database column name or object property used for filtering.
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
    public static void OnFilterValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswFilterBox stsw)
            return;

        /// create param name by removing non-alphanumeric characters
        stsw.SqlParam = "@" + new string([.. ((string)e.NewValue).Where(char.IsLetterOrDigit)]);
        OnValueChanged(stsw, new DependencyPropertyChangedEventArgs());
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
    /// Gets or sets a value indicating whether the drop-down menu is currently open.
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
    /// Indicates whether the control is inside a StswDataGrid.
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
    public static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswFilterBox stsw)
            return;

        if (e.NewValue?.GetType()?.IsListType(out var innerType) == true)
        {
            if (innerType?.IsAssignableTo(typeof(IStswSelectionItem)) != true)
                throw new Exception($"{nameof(ItemsSource)} of {nameof(StswFilterBox)} has to implement {nameof(IStswSelectionItem)} interface!");

            /// short usage for StswComboItem
            if (innerType?.IsAssignableTo(typeof(StswComboItem)) == true)
            {
                if (string.IsNullOrEmpty(stsw.DisplayMemberPath))
                    stsw.DisplayMemberPath = nameof(StswComboItem.Display);
                if (string.IsNullOrEmpty(stsw.SelectedValuePath))
                    stsw.SelectedValuePath = nameof(StswComboItem.Value);
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
    /// Gets the parameter name used in the generated SQL filter query.
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
    /// Gets the generated SQL WHERE clause used for filtering data.
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
    /// Gets or sets the first filter value (used for most conditions like Equals, GreaterThan).
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
    public static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswFilterBox stsw)
            return;

        var filtersType = stsw._dataGrid?.FiltersType;
        if (filtersType == StswDataGridFiltersType.CollectionView)
        {
            stsw._dataGrid?.RegisterExternalFilter(stsw, stsw.GenerateFilterPredicate());
        }
        else if (filtersType == StswDataGridFiltersType.SQL)
        {
            var needsTwo = stsw.FilterMode == StswFilterMode.Between;
            var hasList = stsw.FilterMode is StswFilterMode.In or StswFilterMode.NotIn;

            if ((needsTwo && (stsw.Value1 == null || stsw.Value2 == null)) ||
                (!needsTwo && !hasList && stsw.Value1 == null) ||
                (hasList && (stsw.ItemsSource?.OfType<IStswSelectionItem>().Any(x => x.IsSelected) != true)))
            {
                stsw.SqlString = null;
            }
            else
            {
                stsw.GenerateSqlString();
            }
        }

        stsw.RaiseEvent(new RoutedEventArgs(FilterChangedEvent));
    }
    internal object? DefaultValue1 { get; set; } = null;

    /// <summary>
    /// Gets or sets the second filter value (used in range-based conditions like Between).
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
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
    /// Gets or sets the thickness of the separator between the filter box and dropdown button.
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
