﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control that combines the functionality of a <see cref="ComboBox"/> and <see cref="ListBox"/> to allow multiple selection.
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically binds selected items.
/// </summary>
public class StswSelectionBox : ContentControl, IStswBoxControl, IStswCornerControl, IStswDropControl, IStswScrollableControl
{
    public StswSelectionBox()
    {
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnPreviewMouseDownOutsideCapturedElement);
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswSelectionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSelectionBox), new FrameworkPropertyMetadata(typeof(StswSelectionBox)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// ListBox
        if (GetTemplateChild("PART_ListBox") is StswListBox listBox)
            listBox.SelectionChanged += (s, e) => SetText();

        /// SetTextCommand
        SetTextCommand ??= new StswCommand(SetText);
    }

    /// <summary>
    /// Sets the text for the control based on the selected items.
    /// </summary>
    internal void SetText()
    {
        if (ItemsSource == null)
            return;

        if (ItemsSource?.GetType()?.IsListType(out var innerType) != true || innerType?.IsAssignableTo(typeof(IStswSelectionItem)) != true)
            throw new Exception($"{nameof(ItemsSource)} of {nameof(StswSelectionBox)} has to implement {nameof(IStswSelectionItem)} interface!");

        var itemsSource = ItemsSource.OfType<IStswSelectionItem>().ToList();
        var listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
        var selectedText = new StringBuilder();

        SelectedItems = itemsSource.Where(x => x.IsSelected).ToList();
        foreach (var selectedItem in SelectedItems)
        {
            if (DisplayMemberPath != null && selectedItem.GetType().GetProperty(DisplayMemberPath) is PropertyInfo propertyInfo)
            {
                var value = propertyInfo.GetValue(selectedItem);
                selectedText.Append(value?.ToString());
                selectedText.Append(listSeparator);
            }
            else
            {
                selectedText.Append(selectedItem);
                selectedText.Append(listSeparator);
            }
        }
        if (selectedText.Length > listSeparator.Length)
            selectedText.Length -= listSeparator.Length;

        Text = selectedText.ToString();
    }

    /// <summary>
    /// Gets a <see cref="StswPopup"/> of the control.
    /// </summary>
    public StswPopup GetPopup() => (StswPopup)Template.FindName("PART_Popup", this);
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the path to the display string property of the items in the ItemsSource.
    /// </summary>
    public string? DisplayMemberPath
    {
        get => (string?)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }
    public static readonly DependencyProperty DisplayMemberPathProperty
        = DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(StswSelectionBox)
        );

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
            typeof(StswSelectionBox)
        );

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
            typeof(StswSelectionBox)
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
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsDropDownOpenChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnIsDropDownOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswSelectionBox stsw)
        {
            if (stsw.IsDropDownOpen)
                _ = Mouse.Capture(stsw, CaptureMode.SubTree);
            else
                _ = Mouse.Capture(null);
        }
    }
    private void OnPreviewMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e) => SetCurrentValue(IsDropDownOpenProperty, false);

    /// <summary>
    /// Gets or sets a value indicating whether the drop button is in read-only mode.
    /// When set to true, the popup with items is accessible, but all items within the popup are disabled.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the collection that is used to generate the content of the control.
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
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(IList),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsSourceChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswSelectionBox stsw)
        {
            stsw.SetText();
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
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the path to the value property of the selected items in the ItemsSource.
    /// </summary>
    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        internal set => SetValue(SelectedItemsProperty, value);
    }
    public static readonly DependencyProperty SelectedItemsProperty
        = DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the path to the value property of the selected items in the ItemsSource.
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

    /// <summary>
    /// Gets or sets the text value of the control.
    /// </summary>
    public ICommand SetTextCommand
    {
        get => (ICommand)GetValue(SetTextCommandProperty);
        set => SetValue(SetTextCommandProperty, value);
    }
    public static readonly DependencyProperty SetTextCommandProperty
        = DependencyProperty.Register(
            nameof(SetTextCommand),
            typeof(ICommand),
            typeof(StswSelectionBox)
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
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the text value of the control.
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
            typeof(StswSelectionBox)
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
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the maximum height of the drop-down portion of the control.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the data model for properties of the dropdown popup associated with the control.
    /// The <see cref="StswPopupModel"/> class provides customization options for the appearance and behavior of the popup.
    /// </summary>
    public StswPopupModel Popup
    {
        get => (StswPopupModel)GetValue(PopupProperty);
        set => SetValue(PopupProperty, value);
    }
    public static readonly DependencyProperty PopupProperty
        = DependencyProperty.Register(
            nameof(Popup),
            typeof(StswPopupModel),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the data model for properties of the scroll viewer associated with the control.
    /// The <see cref="StswScrollViewerModel"/> class provides customization options for the appearance and behavior of the scroll viewer.
    /// </summary>
    public StswScrollViewerModel ScrollViewer
    {
        get => (StswScrollViewerModel)GetValue(ScrollViewerProperty);
        set => SetValue(ScrollViewerProperty, value);
    }
    public static readonly DependencyProperty ScrollViewerProperty
        = DependencyProperty.Register(
            nameof(ScrollViewer),
            typeof(StswScrollViewerModel),
            typeof(StswSelectionBox)
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
            typeof(StswSelectionBox)
        );
    #endregion
}
