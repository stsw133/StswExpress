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

namespace StswExpress;
/// <summary>
/// Represents a control that allows selection from a drop-down list of items.
/// </summary>
public class StswComboBox : ComboBox, IStswBoxControl, IStswCornerControl, IStswDropControl, IStswSelectionControl
{
    public StswComboBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
        DependencyPropertyDescriptor.FromProperty(IsDropDownOpenProperty, typeof(ComboBox)).AddValueChanged(this, OnIsDropDownOpenChanged);
    }
    static StswComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComboBox), new FrameworkPropertyMetadata(typeof(StswComboBox)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswComboBox), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private TextBoxBase? _filter;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Filter
        if (GetTemplateChild("PART_Filter") is TextBoxBase filter)
            _filter = filter;
        /// Popup
        if (GetTemplateChild("PART_Popup") is Popup popup)
        {
            popup.Opened += OnIsDropDownOpenChanged;
            popup.GotFocus += OnIsDropDownOpenChanged;
        }
    }

    /// <summary>
    /// Handles the event when the IsDropDownOpen property changes.
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
    /// <summary>
    /// Overrides the behavior for keyboard input to properly handle filtering when it is enabled.
    /// </summary>
    /// <param name="e">The event arguments</param>
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
    /// <summary>
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);

        if (IsFilterEnabled && newValue != null && newValue is not ICollectionView)
            throw new Exception($"{nameof(ItemsSource)} of {nameof(StswComboBox)} has to implement {nameof(ICollectionView)} interface if filter is enabled!");

        if (oldValue is ICollectionView oldCollectionView)
            oldCollectionView.Filter -= CollectionViewFilter;
        if (newValue is ICollectionView newCollectionView && IsFilterEnabled)
            newCollectionView.Filter += CollectionViewFilter;

        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Occurs when the ItemTemplate property value changes.
    /// </summary>
    /// <param name="oldItemTemplate">The old value of the ItemTemplate property.</param>
    /// <param name="newItemTemplate">The new value of the ItemTemplate property.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <summary>
    /// Occurs when the SelectedItem property value changes.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (IsDropDownOpen && IsFilterEnabled)
            Keyboard.Focus(_filter);
    }

    /// <summary>
    /// Filters the collection view based on the FilterText property.
    /// </summary>
    /// <param name="obj">The object to filter</param>
    /// <returns><see langword="true"/> if the object should be included in the view, otherwise <see langword="false"/></returns>
    private bool CollectionViewFilter(object obj)
    {
        if (string.IsNullOrEmpty(FilterText))
            return true;

        if (!string.IsNullOrEmpty(DisplayMemberPath) && obj.GetType().GetProperty(DisplayMemberPath) is PropertyInfo propertyInfo)
            return propertyInfo.GetValue(obj)?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;
        else
            return obj?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a collection of errors to display in <see cref="StswSubError"/>'s tooltip.
    /// </summary>
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
    /// Gets or sets the text used for filtering the items in list.
    /// </summary>
    /// <remarks>
    /// Setting this property will trigger filtering of the items displayed in list based on the specified text.
    /// </remarks>
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
    public static void OnFilterTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswComboBox stsw)
        {
            if (stsw.ItemsSource is ICollectionView collectionView && stsw.IsFilterEnabled)
                collectionView.Refresh();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="StswSubError"/> is visible within the box when there is at least one validation error.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the icon section of the box.
    /// </summary>
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
    /// Gets or sets a value indicating whether filtering of items in list is enabled.
    /// </summary>
    /// <remarks>
    /// Enabling filtering allows the control to filter its items based on the text specified in the <see cref="FilterText"/> property.
    /// </remarks>
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
    public static void OnIsFilterEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswComboBox stsw)
        {
            if (stsw.IsFilterEnabled && stsw.ItemsSource != null && stsw.ItemsSource is not ICollectionView)
                throw new Exception($"{nameof(ItemsSource)} of {nameof(StswComboBox)} has to implement {nameof(ICollectionView)} interface if filter is enabled!");

            if (stsw.ItemsSource is ICollectionView collectionView)
            {
                if (stsw.IsFilterEnabled)
                    collectionView.Filter += stsw.CollectionViewFilter;
                else
                    collectionView.Filter -= stsw.CollectionViewFilter;
            }
        }
    }

    /// <summary>
    /// Gets or sets the placeholder text displayed in the control when no item is selected.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the collection of sub controls to be displayed in the control.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the control uses selection items that implement the <see cref="IStswSelectionItem"/> interface.
    /// </summary>
    public bool UsesSelectionItems
    {
        get => (bool)GetValue(UsesSelectionItemsProperty);
        set => SetValue(UsesSelectionItemsProperty, value);
    }
    public static readonly DependencyProperty UsesSelectionItemsProperty
        = DependencyProperty.Register(
            nameof(UsesSelectionItems),
            typeof(bool),
            typeof(StswComboBox)
        );
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
            typeof(StswComboBox),
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
            typeof(StswComboBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between arrow icon and main button.
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
            typeof(StswComboBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
