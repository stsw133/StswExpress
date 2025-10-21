using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;/// <summary>
/// A combo box control that allows users to select an item from a drop-down list.
/// Supports filtering, selection binding, and corner customization.
/// This control allows filtering of items based on user input and supports displaying icons and custom placeholders.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswComboBox ItemsSource="{Binding Cities}" IsFilterEnabled="True" Placeholder="Select a city"/&gt;
/// </code>
/// </example>
public class StswComboBox : ComboBox, IStswBoxControl, IStswCornerControl, IStswDropControl, IStswSelectionControl
{
    private TextBoxBase? _filter;
    private ICollectionView? _itemsView;
    bool IStswDropControl.SuppressNextOpen { get; set; }

    public StswComboBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
        DependencyPropertyDescriptor.FromProperty(IsDropDownOpenProperty, typeof(ComboBox)).AddValueChanged(this, OnIsDropDownOpenChanged);
    }
    static StswComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComboBox), new FrameworkPropertyMetadata(typeof(StswComboBox)));
        StswControl.OverrideBaseBorderThickness<StswComboBox>(getExt: c => c.BorderThickness, setExt: (c, st) => c.BorderThickness = st);
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswComboBoxItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswComboBoxItem;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _filter = GetTemplateChild("PART_Filter") as TextBoxBase;
        
        if (GetTemplateChild("PART_Popup") is Popup popup)
        {
            popup.Opened += OnIsDropDownOpenChanged;
            popup.GotFocus += OnIsDropDownOpenChanged;
        }
    }

    /// <summary>
    /// Handles the event when the drop-down opens or closes. 
    /// If filtering is enabled, focuses the filter input field.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnIsDropDownOpenChanged(object? sender, EventArgs e)
    {
        if (IsDropDownOpen && IsFilterEnabled)
            Keyboard.Focus(_filter);
        else if (IsDropDownOpen && IsEditable)
            Keyboard.Focus(this);
    }
    /*
    /// <inheritdoc/>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (_filter?.IsKeyboardFocused == true)
        {
            switch (e.Key)
            {
                case Key.Down:
                case Key.Up:
                case Key.Tab:
                    (Keyboard.FocusedElement as UIElement)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                    break;
            }
        }
        else if (_filter?.IsKeyboardFocused == false)
        {
            if (Items.CurrentPosition == 0 && e.Key == Key.Up)
            {
                Keyboard.Focus(_filter);
                e.Handled = true;
            }
            else if (Items.CurrentPosition == Items.Count - 1 && e.Key == Key.Down)
            {
                Keyboard.Focus(_filter);
                e.Handled = true;
            }
        }
    }
    */
    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);

        DetachFilter();
        _itemsView = newValue != null ? CollectionViewSource.GetDefaultView(newValue) : null;

        if (IsFilterEnabled)
            AttachFilter();
        else
            _itemsView?.Refresh();

        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <inheritdoc/>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <inheritdoc/>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (!IStswSelectionControl.PreviewKeyDown(this, e)) return;
        base.OnPreviewKeyDown(e);
    }

    /// <inheritdoc/>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        // causes problem with starting selected value (to verify why it existed)
        //if (IsReadOnly)
        //{
        //    e.Handled = true;
        //    return;
        //}

        base.OnSelectionChanged(e);
        IStswSelectionControl.SelectionChanged(this, e.AddedItems, e.RemovedItems);

        if (IsDropDownOpen && IsFilterEnabled)
            Keyboard.Focus(_filter);

        /// moved here from StswComboItem, cause was bugged with multiple instances binded to same ItemsSource
        if (SelectedItem is IStswSelectionItem item)
            item.IsSelected = true;
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StswComboBoxItem listBoxItem)
        {
            listBoxItem.SetBinding(StswComboBoxItem.IsReadOnlyProperty, new Binding(nameof(IsReadOnly))
            {
                Source = this,
                Mode = BindingMode.OneWay
            });
        }
    }

    /// <summary>
    /// Filters the collection based on <see cref="FilterText"/> and <see cref="FilterMemberPath"/>.
    /// Returns <see langword="true"/> if the item matches, otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="obj">The object to filter</param>
    /// <returns><see langword="true"/> if the object should be included, otherwise <see langword="false"/></returns>
    private bool CollectionViewFilter(object obj)
    {
        if (ReferenceEquals(obj, SelectedItem))
            return true;

        if (string.IsNullOrEmpty(FilterText))
            return true;

        if (!string.IsNullOrEmpty(FilterMemberPath) && obj.GetType().GetProperty(FilterMemberPath) is PropertyInfo filterMemberPathProp)
            return filterMemberPathProp.GetValue(obj)?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;
        else if (!string.IsNullOrEmpty(DisplayMemberPath) && obj.GetType().GetProperty(DisplayMemberPath) is PropertyInfo displayMemberPathProp)
            return displayMemberPathProp.GetValue(obj)?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;
        else
            return obj?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;
    }

    /// <summary>
    /// Attaches the filter to the collection view if filtering is enabled.
    /// </summary>
    private void AttachFilter()
    {
        if (_itemsView is null)
            return;

        if (!_itemsView.CanFilter)
        {
            _itemsView.Refresh();
            return;
        }

        _itemsView.Filter -= CollectionViewFilter;
        _itemsView.Filter += CollectionViewFilter;
        _itemsView.Refresh();
    }

    /// <summary>
    /// Detaches the filter from the collection view.
    /// </summary>
    private void DetachFilter()
    {
        if (_itemsView is null || !_itemsView.CanFilter)
            return;

        _itemsView.Filter -= CollectionViewFilter;
    }
    #endregion

    #region Logic properties
    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty
        = DependencyProperty.Register(
            nameof(Errors),
            typeof(ReadOnlyObservableCollection<ValidationError>),
            typeof(StswComboBox)
        );

    /// <summary>
    /// Gets or sets the member path used for filtering.
    /// </summary>
    public string FilterMemberPath
    {
        get => (string)GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }
    public static readonly DependencyProperty FilterMemberPathProperty
        = DependencyProperty.Register(
            nameof(FilterMemberPath),
            typeof(string),
            typeof(StswComboBox)
        );

    /// <summary>
    /// Gets or sets the text used for filtering the items in list.
    /// </summary>
    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }
    public static readonly DependencyProperty FilterTextProperty
        = DependencyProperty.Register(
            nameof(FilterText),
            typeof(string),
            typeof(StswComboBox),
            new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFilterTextChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswComboBox stsw)
            return;

        if (stsw.IsFilterEnabled)
            stsw._itemsView?.Refresh();
    }

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty
        = DependencyProperty.Register(
            nameof(HasError),
            typeof(bool),
            typeof(StswComboBox)
        );

    /// <inheritdoc/>
    public object? Icon
    {
        get => (object?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(StswComboBox)
        );

    /// <summary>
    /// Gets or sets whether filtering is enabled.
    /// When enabled, the control will filter items based on <see cref="FilterText"/> and <see cref="FilterMemberPath"/>.
    /// </summary>
    public bool IsFilterEnabled
    {
        get => (bool)GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }
    public static readonly DependencyProperty IsFilterEnabledProperty
        = DependencyProperty.Register(
            nameof(IsFilterEnabled),
            typeof(bool),
            typeof(StswComboBox),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsFilterEnabledChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsFilterEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswComboBox stsw)
            return;

        stsw.DetachFilter();

        if (stsw.IsFilterEnabled)
            stsw.AttachFilter();
        else
            stsw._itemsView?.Refresh();
    }

    /// <inheritdoc/>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswComboBox)
        );

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty
        = DependencyProperty.Register(
            nameof(SubControls),
            typeof(ObservableCollection<IStswSubControl>),
            typeof(StswComboBox)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether the drop-down button area should be visible.
    /// </summary>
    public bool AreButtonsVisible
    {
        get => (bool)GetValue(AreButtonsVisibleProperty);
        set => SetValue(AreButtonsVisibleProperty, value);
    }
    public static readonly DependencyProperty AreButtonsVisibleProperty
        = DependencyProperty.Register(
            nameof(AreButtonsVisible),
            typeof(bool),
            typeof(StswComboBox),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange)
        );

    /// <summary>
    /// Gets or sets the thickness of the border, including the inner separator value.
    /// </summary>
    public new StswThickness BorderThickness
    {
        get => (StswThickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    public new static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.Register(
            nameof(BorderThickness),
            typeof(StswThickness),
            typeof(StswComboBox),
            new FrameworkPropertyMetadata(default(StswThickness),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                StswControl.CreateExtendedChangedCallback<StswComboBox>((c, th) => c.SetCurrentValue(Control.BorderThicknessProperty, th)))
        );

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
            typeof(StswComboBox),
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
            typeof(StswComboBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public double MaxDropDownWidth
    {
        get => (double)GetValue(MaxDropDownWidthProperty);
        set => SetValue(MaxDropDownWidthProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownWidthProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownWidth),
            typeof(double),
            typeof(StswComboBox),
            new PropertyMetadata(double.NaN)
        );
    #endregion
}
