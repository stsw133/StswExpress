using System.Collections;
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
public class StswFilterBox : Control, IStswCornerControl
{
    public ICommand SelectModeCommand { get; set; }

    public StswFilterBox()
    {
        SelectModeCommand = new StswCommand<StswFilterMode>(SelectMode);
    }
    static StswFilterBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFilterBox), new FrameworkPropertyMetadata(typeof(StswFilterBox)));
    }

    #region Events & methods
    private ButtonBase? filterMode;
    private StswDataGrid? stswDataGrid;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (StswFn.FindVisualAncestor<StswDataGrid>(this) is StswDataGrid stswDataGrid)
        {
            this.stswDataGrid = stswDataGrid;
            IsInDataGrid = true;
        }

        /// ToggleButton: filter mode
        if (GetTemplateChild("PART_FilterMode") is ButtonBase filterMode)
            this.filterMode = filterMode;

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
                StswAdaptiveType.Check => StswFilterMode.Equal,
                StswAdaptiveType.Date => StswFilterMode.Equal,
                StswAdaptiveType.List => StswFilterMode.In,
                StswAdaptiveType.Number => StswFilterMode.Equal,
                StswAdaptiveType.Text => StswFilterMode.Contains,
                _ => null
            };
        }
        else OnFilterModeChanged(this, new DependencyPropertyChangedEventArgs());

        /// assign default values
        DefaultFilterMode = FilterMode;
        DefaultValue1 = Value1;
        DefaultValue2 = Value2;

        OnValueChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// Command: select mode
    /// <summary>
    /// Event handler for executing when the filter mode is selected.
    /// </summary>
    protected void SelectMode(StswFilterMode parameter) => FilterMode = parameter;

    /// <summary>
    /// Event handler for handling the KeyDown event.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Enter)
            stswDataGrid?.RefreshCommand?.Execute(null);
    }
    #endregion

    #region Main properties
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
            if (stsw.FilterMode != null
             && stsw.filterMode?.Content is StswOutlinedText symbolBlock and not null
             && stsw.filterMode?.ContextMenu?.Items?.OfType<MenuItem>()?.ToList()?[(int)stsw.FilterMode]?.Icon is StswOutlinedText newSymbolBlock and not null)
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
    /// Gets or sets the visibility of filter mode.
    /// </summary>
    public Visibility FilterModeVisibility
    {
        get => (Visibility)GetValue(FilterModeVisibilityProperty);
        set => SetValue(FilterModeVisibilityProperty, value);
    }
    public static readonly DependencyProperty FilterModeVisibilityProperty
        = DependencyProperty.Register(
            nameof(FilterModeVisibility),
            typeof(Visibility),
            typeof(StswFilterBox)
        );

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
    /// Gets or sets a value indicating whether the drop-down portion of the filter is open.
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
    public static readonly DependencyProperty IsInDataGridProperty
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
            typeof(StswFilterBox)
        );
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
    /// 
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
            typeof(StswFilterBox)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswFilterBox)
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
            typeof(StswFilterBox)
        );
    #endregion
}
