using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress;/// <summary>
/// A multi-selection combo box that allows users to select multiple items from a drop-down list.
/// Supports item binding, selection tracking, drop-down customization, and error indication.
/// </summary>
/// <remarks>
/// The <see cref="ItemsSource"/> must contain elements implementing <see cref="IStswSelectionItem"/>.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSelectionBox ItemsSource="{Binding Tags}" Placeholder="Select tags"/&gt;
/// </code>
/// </example>
public class StswSelectionBox : ItemsControl, IStswBoxControl, IStswCornerControl, IStswDropControl
{
    private readonly HashSet<object> _hiddenSelectedItems = [];
    private ICollectionView? _itemsView;
    private TextBoxBase? _filter;
    private ListBox? _listBox;
    private Popup? _popup;

    bool IStswDropControl.SuppressNextOpen { get; set; }

    public StswSelectionBox()
    {
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, IStswDropControl.PreviewMouseDownOutsideCapturedElement);
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswSelectionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSelectionBox), new FrameworkPropertyMetadata(typeof(StswSelectionBox)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UpdateTextCommand ??= new StswCommand(UpdateText); // ensure the command is initialized

        if (_popup != null)
        {
            _popup.Opened -= OnDropDownOpened;
            _popup.GotFocus -= OnDropDownOpened;
        }
        if (_listBox != null)
            _listBox.SelectionChanged -= ListBox_SelectionChanged;

        /// filter textbox
        _filter = GetTemplateChild("PART_Filter") as TextBoxBase;

        /// popup
        _popup = GetTemplateChild("PART_Popup") as Popup;
        if (_popup != null)
        {
            _popup.Opened += OnDropDownOpened;
            _popup.GotFocus += OnDropDownOpened;
        }

        /// listbox
        _listBox = GetTemplateChild("PART_ListBox") as ListBox;
        if (_listBox != null)
            _listBox.SelectionChanged += ListBox_SelectionChanged;
    }

    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        if (newValue?.GetType()?.IsListType(out var innerType) == true)
        {
            if (innerType?.IsAssignableTo(typeof(IStswSelectionItem)) != true)
                throw new InvalidOperationException($"{nameof(StswSelectionBox)} ItemsSource must contain objects implementing {nameof(IStswSelectionItem)}!");

            /// Optional: If using StswComboItem (short usage), set defaults
            if (innerType?.IsAssignableTo(typeof(StswComboItem)) == true)
            {
                if (string.IsNullOrEmpty(DisplayMemberPath) && ItemTemplate == null)
                    DisplayMemberPath = nameof(StswComboItem.Display);
                if (string.IsNullOrEmpty(SelectedValuePath))
                    SelectedValuePath = nameof(StswComboItem.Value);
            }
        }

        DetachFilter();
        ShowHiddenSelectedItems();
        _itemsView = newValue != null ? CollectionViewSource.GetDefaultView(newValue) : null;

        if (IsFilterEnabled)
            AttachFilter();
        else
            _itemsView?.Refresh();

        base.OnItemsSourceChanged(oldValue, newValue);

        /// refresh displayed text whenever the ItemsSource changes.
        UpdateTextCommand?.Execute(null);
        UpdateSelectedItemsVisibility();
    }

    /// <inheritdoc/>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        if (newItemTemplate != null && !string.IsNullOrEmpty(DisplayMemberPath))
            DisplayMemberPath = string.Empty;
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <summary>
    /// Handles the event when the drop-down opens or gains focus.
    /// If filtering is enabled, focuses the filter input field.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnDropDownOpened(object? sender, EventArgs e)
    {
        if (IsDropDownOpen && IsFilterEnabled)
            Keyboard.Focus(_filter);
    }

    /// <summary>
    /// Handles selection changes in the internal ListBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateTextCommand?.Execute(null);
        UpdateSelectedItemsVisibility();
    }

    /// <summary>
    /// Updates the displayed text based on the selected items.
    /// Also synchronizes the internal selection state.
    /// </summary>
    internal void UpdateText()
    {
        if (ItemsSource == null)
        {
            Text = string.Empty;
            return;
        }

        if (_popup?.IsLoaded == true && _listBox?.IsLoaded == false)
            return;

        var itemsSource = ItemsSource.OfType<IStswSelectionItem>();

        /// build text from all selected items
        var newlySelected = new ObservableCollection<IStswSelectionItem>();
        var sb = new StringBuilder();

        /// use the local list separator (e.g. ", ")
        var listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

        foreach (var selectedItem in itemsSource.Where(x => x.IsSelected))
        {
            newlySelected.Add(selectedItem);

            /// if we have a DisplayMemberPath, try to get that property
            var value = !string.IsNullOrEmpty(DisplayMemberPath)
                ? selectedItem.GetPropertyValue(DisplayMemberPath)?.ToString()
                : selectedItem.ToString();

            if (!string.IsNullOrEmpty(value))
                sb.Append(value).Append(listSeparator);
        }

        /// remove the trailing separator if needed
        if (sb.Length >= listSeparator.Length)
            sb.Length -= listSeparator.Length;

        /// final text
        Text = sb.ToString();
    }
    #endregion

    #region Filter logic
    /// <summary>
    /// Filters the collection based on <see cref="FilterText"/> and <see cref="FilterMemberPath"/>.
    /// Returns <see langword="true"/> if the item matches, otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="obj">The object to filter.</param>
    /// <returns><see langword="true"/> if the object should be included, otherwise <see langword="false"/>.</returns>
    private bool CollectionViewFilter(object obj)
    {
        if (obj is IStswSelectionItem selectionItem && selectionItem.IsSelected && !HideSelectedItemWhenFiltered)
            return true;

        return MatchesFilter(obj);
    }

    /// <summary>
    /// Determines if the given object matches the current filter criteria.
    /// </summary>
    /// <param name="obj">The object to check against the filter.</param>
    /// <returns><see langword="true"/> if the object matches the filter; otherwise, <see langword="false"/>.</returns>
    private bool MatchesFilter(object obj)
    {
        if (string.IsNullOrEmpty(FilterText))
            return true;

        if (!string.IsNullOrEmpty(FilterMemberPath))
            return obj.GetPropertyValue(FilterMemberPath)?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;
        if (!string.IsNullOrEmpty(DisplayMemberPath))
            return obj.GetPropertyValue(DisplayMemberPath)?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;

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

    /// <summary>
    /// Updates the visibility of selected items based on the current filter state and <see cref="HideSelectedItemWhenFiltered"/> property.
    /// </summary>
    private void UpdateSelectedItemsVisibility()
    {
        if (!HideSelectedItemWhenFiltered || !IsFilterEnabled || string.IsNullOrEmpty(FilterText))
        {
            ShowHiddenSelectedItems();
            return;
        }

        var selectedItems = ItemsSource?.OfType<IStswSelectionItem>().Where(x => x.IsSelected).Cast<object>().ToList();
        if (selectedItems is null)
        {
            ShowHiddenSelectedItems();
            return;
        }

        var itemsToHide = selectedItems.Where(x => !MatchesFilter(x)).ToList();

        foreach (var item in itemsToHide)
            HideSelectedItem(item);

        foreach (var item in _hiddenSelectedItems.ToList())
        {
            if (!itemsToHide.Contains(item))
                ShowHiddenSelectedItem(item);
        }
    }

    /// <summary>
    /// Hides the specified selected item by collapsing its container.
    /// </summary>
    /// <param name="item">The selected item to hide.</param>
    private void HideSelectedItem(object item)
    {
        _hiddenSelectedItems.Add(item);
        SetContainerVisibility(item, Visibility.Collapsed);
    }

    /// <summary>
    /// Shows all previously hidden selected items.
    /// </summary>
    private void ShowHiddenSelectedItems()
    {
        foreach (var hiddenItem in _hiddenSelectedItems.ToList())
            ShowHiddenSelectedItem(hiddenItem);
    }

    /// <summary>
    /// Shows the specified previously hidden item.
    /// </summary>
    /// <param name="item">The item to show.</param>
    private void ShowHiddenSelectedItem(object item)
    {
        SetContainerVisibility(item, Visibility.Visible);
        _hiddenSelectedItems.Remove(item);
        _itemsView?.Refresh();
    }

    /// <summary>
    /// Sets the visibility of the container corresponding to the specified item.
    /// </summary>
    /// <param name="item">The item whose container's visibility is to be set.</param>
    /// <param name="visibility">The desired visibility state.</param>
    private void SetContainerVisibility(object? item, Visibility visibility)
    {
        if (item is null || _listBox is null)
            return;

        void ApplyVisibility()
        {
            if (_listBox.ItemContainerGenerator.ContainerFromItem(item) is UIElement element)
                element.Visibility = visibility;
        }

        if (_listBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            ApplyVisibility();

            if (_listBox.ItemContainerGenerator.ContainerFromItem(item) is null)
                _listBox.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(ApplyVisibility));

            return;
        }

        void OnStatusChanged(object? sender, EventArgs e)
        {
            if (_listBox.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return;

            _listBox.ItemContainerGenerator.StatusChanged -= OnStatusChanged;
            ApplyVisibility();
        }

        _listBox.ItemContainerGenerator.StatusChanged += OnStatusChanged;
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
            typeof(StswSelectionBox)
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
            typeof(StswSelectionBox)
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
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFilterTextChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswSelectionBox stsw)
            return;

        if (stsw.IsFilterEnabled)
            stsw._itemsView?.Refresh();

        stsw.UpdateSelectedItemsVisibility();
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
            typeof(StswSelectionBox)
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
            typeof(StswSelectionBox)
        );

    /// <inheritdoc/>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsDropDownOpenChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => IStswDropControl.IsDropDownOpenChanged(d, e);

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
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsFilterEnabledChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsFilterEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswSelectionBox stsw)
            return;

        stsw.DetachFilter();

        if (stsw.IsFilterEnabled)
            stsw.AttachFilter();
        else
            stsw._itemsView?.Refresh();

        stsw.UpdateSelectedItemsVisibility();
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswSelectionBox)
        );

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
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the property path used to retrieve the value of selected items.
    /// </summary>
    public string? SelectedValuePath
    {
        get => (string?)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }
    public static readonly DependencyProperty SelectedValuePathProperty
        = DependencyProperty.Register(
            nameof(SelectedValuePath),
            typeof(string),
            typeof(StswSelectionBox)
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
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the text representation of the selected items.
    /// This property updates dynamically based on selection changes.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.None,
                null, null, false, UpdateSourceTrigger.PropertyChanged)
        );

    /// <summary>
    /// Gets or sets the command that updates the displayed text based on selected items.
    /// </summary>
    public ICommand UpdateTextCommand
    {
        get => (ICommand)GetValue(UpdateTextCommandProperty);
        set => SetValue(UpdateTextCommandProperty, value);
    }
    public static readonly DependencyProperty UpdateTextCommandProperty
        = DependencyProperty.Register(
            nameof(UpdateTextCommand),
            typeof(ICommand),
            typeof(StswSelectionBox)
        );
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
            typeof(StswSelectionBox),
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
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the selected item should be hidden from the filtered list when it does not match the filter criteria.
    /// When enabled, selected items remain selected even if they are not shown.
    /// </summary>
    public bool HideSelectedItemWhenFiltered
    {
        get => (bool)GetValue(HideSelectedItemWhenFilteredProperty);
        set => SetValue(HideSelectedItemWhenFilteredProperty, value);
    }
    public static readonly DependencyProperty HideSelectedItemWhenFilteredProperty
        = DependencyProperty.Register(
            nameof(HideSelectedItemWhenFiltered),
            typeof(bool),
            typeof(StswSelectionBox),
            new PropertyMetadata(true, OnHideSelectedItemWhenFilteredChanged)
        );
    private static void OnHideSelectedItemWhenFilteredChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswSelectionBox stsw)
            return;

        stsw.UpdateSelectedItemsVisibility();
    }

    /// <inheritdoc/>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownHeightProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(StswSelectionBox),
            new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 3)
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
            typeof(StswSelectionBox),
            new PropertyMetadata(double.NaN)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the drop-down button and the main input field.
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
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
