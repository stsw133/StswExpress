﻿using System;
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
[StswInfo(null)]
public class StswComboBox : ComboBox, IStswBoxControl, IStswCornerControl, IStswDropControl, IStswSelectionControl
{
    private TextBoxBase? _filter;
    bool IStswDropControl.SuppressNextOpen { get; set; }

    public StswComboBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
        DependencyPropertyDescriptor.FromProperty(IsDropDownOpenProperty, typeof(ComboBox)).AddValueChanged(this, OnIsDropDownOpenChanged);
    }
    static StswComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComboBox), new FrameworkPropertyMetadata(typeof(StswComboBox)));
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
    [StswInfo("0.7.0")]
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
    [StswInfo("0.10.0")]
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

    /// <inheritdoc/>
    [StswInfo("0.10.0")]
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <inheritdoc/>
    [StswInfo("0.17.0")]
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (!IStswSelectionControl.PreviewKeyDown(this, e)) return;
        base.OnPreviewKeyDown(e);
    }

    /// <inheritdoc/>
    [StswInfo("0.10.0")]
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
    [StswInfo("0.14.0")]
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
    [StswInfo("0.7.0")]
    private bool CollectionViewFilter(object obj)
    {
        if (string.IsNullOrEmpty(FilterText))
            return true;

        if (!string.IsNullOrEmpty(FilterMemberPath) && obj.GetType().GetProperty(FilterMemberPath) is PropertyInfo filterMemberPathProp)
            return filterMemberPathProp.GetValue(obj)?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;
        else if (!string.IsNullOrEmpty(DisplayMemberPath) && obj.GetType().GetProperty(DisplayMemberPath) is PropertyInfo displayMemberPathProp)
            return displayMemberPathProp.GetValue(obj)?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;
        else
            return obj?.ToString()?.ToLower()?.Contains(FilterText?.ToLower() ?? string.Empty) == true;
    }
    #endregion

    #region Logic properties
    /// <inheritdoc/>
    [StswInfo("0.6.1")]
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
    [StswInfo("0.14.0")]
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
    [StswInfo("0.7.0")]
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
        if (obj is not StswComboBox stsw)
            return;

        if (stsw.ItemsSource is ICollectionView collectionView && stsw.IsFilterEnabled)
            collectionView.Refresh();
    }

    /// <inheritdoc/>
    [StswInfo("0.6.1")]
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
    [StswInfo("0.12.0")]
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
    /// Requires <see cref="ICollectionView"/> as <see cref="ItemsSource"/>.
    /// </summary>
    [StswInfo("0.7.0")]
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
        if (obj is not StswComboBox stsw)
            return;

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

    /// <inheritdoc/>
    [StswInfo("0.1.0")]
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
    [StswInfo("0.15.0")]
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

    /// <summary>
    /// Gets or sets the thickness of the separator between the arrow icon and main button.
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
