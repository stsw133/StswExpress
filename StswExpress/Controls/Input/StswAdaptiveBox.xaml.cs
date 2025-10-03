using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A dynamic input control that automatically selects the appropriate input box based on the value type.
/// Supports text, number, date, checkbox, and selection inputs.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswAdaptiveBox Type="Text" Value="{Binding UserName}" Placeholder="Enter name"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Value))]
[StswPlannedChanges(StswPlannedChanges.Refactor)]
public class StswAdaptiveBox : Control, IStswBoxControl, IStswCornerControl
{
    private ContentPresenter? _contentPresenter;

    public StswAdaptiveBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswAdaptiveBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswAdaptiveBox), new FrameworkPropertyMetadata(typeof(StswAdaptiveBox)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;

        OnTypeChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Dynamically creates and assigns the appropriate input control based on the specified <see cref="Type"/>.
    /// Ensures correct bindings and properties are applied.
    /// </summary>
    protected void CreateControlBasedOnType()
    {
        if (_contentPresenter == null || Type == StswAdaptiveType.Auto)
            return;

        if (_contentPresenter.Content is FrameworkElement oldControl)
        {
            BindingOperations.ClearAllBindings(oldControl);
            if (oldControl.DataContext is DependencyObject dataContext)
                BindingOperations.ClearAllBindings(dataContext);
        }

        _contentPresenter.Content = null;

        var bindingBorderBrush = new Binding(nameof(BorderBrush)) { Source = this };
        var bindingBorderThickness = new Binding(nameof(BorderThickness)) { Source = this };
        var bindingCornerClipping = new Binding(nameof(CornerClipping)) { Source = this };
        var bindingCornerRadius = new Binding(nameof(CornerRadius)) { Source = this };
        var bindingDisplayMemberPath = new Binding(nameof(DisplayMemberPath)) { Source = this };
        var bindingFormat = new Binding(nameof(Format)) { Source = this };
        var bindingIcon = new Binding(nameof(Icon)) { Source = this };
        var bindingIsReadOnly = new Binding(nameof(IsReadOnly)) { Source = this };
        var bindingIsThreeState = new Binding(nameof(IsThreeState)) { Source = this };
        var bindingItemsSource = new Binding(nameof(ItemsSource)) { Source = this };
        var bindingPadding = new Binding(nameof(Padding)) { Source = this };
        var bindingPlaceholder = new Binding(nameof(Placeholder)) { Source = this };
        var bindingPopupBackground = new Binding { Path = new PropertyPath(StswPopup.BackgroundProperty), Source = this };
        var bindingPopupBorderBrush = new Binding { Path = new PropertyPath(StswPopup.BorderBrushProperty), Source = this };
        var bindingPopupBorderThickness = new Binding { Path = new PropertyPath(StswPopup.BorderThicknessProperty), Source = this };
        var bindingPopupCornerClipping = new Binding { Path = new PropertyPath(StswPopup.CornerClippingProperty), Source = this };
        var bindingPopupCornerRadius = new Binding { Path = new PropertyPath(StswPopup.CornerRadiusProperty), Source = this };
        var bindingPopupPadding = new Binding { Path = new PropertyPath(StswPopup.PaddingProperty), Source = this };
        var bindingSelectedValuePath = new Binding(nameof(SelectedValuePath)) { Source = this };
        var bindingSelectionUnit = new Binding(nameof(SelectionUnit)) { Source = this };
        var bindingSeparatorThickness = new Binding(nameof(SeparatorThickness)) { Source = this };
        var bindingSubControls = new Binding(nameof(SubControls)) { Source = this };
        var bindingHorizontalContentAlignment = new Binding(nameof(HorizontalContentAlignment)) { Source = this };
        var bindingVerticalContentAlignment = new Binding(nameof(VerticalContentAlignment)) { Source = this };
        var bindingValue = new Binding(nameof(Value)) { Source = this, Mode = BindingMode.TwoWay };

        FrameworkElement? newControl = null;
        switch (Type)
        {
            case StswAdaptiveType.Check:
                newControl = new StswCheckBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                newControl.SetBinding(StswCheckBox.BorderBrushProperty, bindingBorderBrush);
                newControl.SetBinding(StswCheckBox.BorderThicknessProperty, bindingBorderThickness);
                newControl.SetBinding(StswCheckBox.CornerClippingProperty, bindingCornerClipping);
                newControl.SetBinding(StswCheckBox.CornerRadiusProperty, bindingCornerRadius);
                newControl.SetBinding(StswCheckBox.IsCheckedProperty, bindingValue);
                newControl.SetBinding(StswCheckBox.IsReadOnlyProperty, bindingIsReadOnly);
                newControl.SetBinding(StswCheckBox.IsThreeStateProperty, bindingIsThreeState);
                newControl.SetBinding(StswCheckBox.PaddingProperty, bindingPadding);
                newControl.SetBinding(StswCheckBox.HorizontalContentAlignmentProperty, bindingHorizontalContentAlignment);
                newControl.SetBinding(StswCheckBox.VerticalContentAlignmentProperty, bindingVerticalContentAlignment);
                break;
            case StswAdaptiveType.Date:
                newControl = new StswDatePicker()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                newControl.SetBinding(StswDatePicker.BorderBrushProperty, bindingBorderBrush);
                newControl.SetBinding(StswDatePicker.BorderThicknessProperty, bindingBorderThickness);
                newControl.SetBinding(StswDatePicker.CornerClippingProperty, bindingCornerClipping);
                newControl.SetBinding(StswDatePicker.CornerRadiusProperty, bindingCornerRadius);
                newControl.SetBinding(StswDatePicker.FormatProperty, bindingFormat);
                newControl.SetBinding(StswDatePicker.IconProperty, bindingIcon);
                newControl.SetBinding(StswDatePicker.IsReadOnlyProperty, bindingIsReadOnly);
                newControl.SetBinding(StswDatePicker.PaddingProperty, bindingPadding);
                newControl.SetBinding(StswDatePicker.PlaceholderProperty, bindingPlaceholder);
                newControl.SetBinding(StswDatePicker.SelectedDateProperty, bindingValue);
                newControl.SetBinding(StswDatePicker.SelectionUnitProperty, bindingSelectionUnit);
                newControl.SetBinding(StswDatePicker.SeparatorThicknessProperty, bindingSeparatorThickness);
                newControl.SetBinding(StswDatePicker.SubControlsProperty, bindingSubControls);
                newControl.SetBinding(StswDatePicker.HorizontalContentAlignmentProperty, bindingHorizontalContentAlignment);
                newControl.SetBinding(StswDatePicker.VerticalContentAlignmentProperty, bindingVerticalContentAlignment);
                break;
            case StswAdaptiveType.List:
                newControl = new StswSelectionBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                newControl.SetBinding(StswSelectionBox.BorderBrushProperty, bindingBorderBrush);
                newControl.SetBinding(StswSelectionBox.BorderThicknessProperty, bindingBorderThickness);
                newControl.SetBinding(StswSelectionBox.CornerClippingProperty, bindingCornerClipping);
                newControl.SetBinding(StswSelectionBox.CornerRadiusProperty, bindingCornerRadius);
                newControl.SetBinding(StswSelectionBox.DisplayMemberPathProperty, bindingDisplayMemberPath);
                newControl.SetBinding(StswSelectionBox.IconProperty, bindingIcon);
                newControl.SetBinding(StswSelectionBox.IsReadOnlyProperty, bindingIsReadOnly);
                newControl.SetBinding(StswSelectionBox.ItemsSourceProperty, bindingItemsSource);
                newControl.SetBinding(StswSelectionBox.PaddingProperty, bindingPadding);
                newControl.SetBinding(StswSelectionBox.PlaceholderProperty, bindingPlaceholder);
                newControl.SetBinding(StswSelectionBox.SelectedValuePathProperty, bindingSelectedValuePath);
                newControl.SetBinding(StswSelectionBox.SeparatorThicknessProperty, bindingSeparatorThickness);
                newControl.SetBinding(StswSelectionBox.SubControlsProperty, bindingSubControls);
                newControl.SetBinding(StswSelectionBox.TextProperty, bindingValue);
                newControl.SetBinding(StswSelectionBox.HorizontalContentAlignmentProperty, bindingHorizontalContentAlignment);
                newControl.SetBinding(StswSelectionBox.VerticalContentAlignmentProperty, bindingVerticalContentAlignment);
                newControl.SetBinding(StswPopup.BackgroundProperty, bindingPopupBackground);
                newControl.SetBinding(StswPopup.BorderBrushProperty, bindingPopupBorderBrush);
                newControl.SetBinding(StswPopup.BorderThicknessProperty, bindingPopupBorderThickness);
                newControl.SetBinding(StswPopup.CornerClippingProperty, bindingPopupCornerClipping);
                newControl.SetBinding(StswPopup.CornerRadiusProperty, bindingPopupCornerRadius);
                newControl.SetBinding(StswPopup.PaddingProperty, bindingPopupPadding);
                break;
            case StswAdaptiveType.Number:
                newControl = new StswDecimalBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                newControl.SetBinding(StswDecimalBox.BorderBrushProperty, bindingBorderBrush);
                newControl.SetBinding(StswDecimalBox.BorderThicknessProperty, bindingBorderThickness);
                newControl.SetBinding(StswDecimalBox.CornerClippingProperty, bindingCornerClipping);
                newControl.SetBinding(StswDecimalBox.CornerRadiusProperty, bindingCornerRadius);
                newControl.SetBinding(StswDecimalBox.FormatProperty, bindingFormat);
                newControl.SetBinding(StswDecimalBox.IconProperty, bindingIcon);
                newControl.SetBinding(StswDecimalBox.IsReadOnlyProperty, bindingIsReadOnly);
                newControl.SetBinding(StswDecimalBox.PaddingProperty, bindingPadding);
                newControl.SetBinding(StswDecimalBox.PlaceholderProperty, bindingPlaceholder);
                newControl.SetBinding(StswDecimalBox.SeparatorThicknessProperty, bindingSeparatorThickness);
                newControl.SetBinding(StswDecimalBox.SubControlsProperty, bindingSubControls);
                newControl.SetBinding(StswDecimalBox.ValueProperty, bindingValue);
                newControl.SetBinding(StswDecimalBox.HorizontalContentAlignmentProperty, bindingHorizontalContentAlignment);
                newControl.SetBinding(StswDecimalBox.VerticalContentAlignmentProperty, bindingVerticalContentAlignment);
                break;
            case StswAdaptiveType.Text:
                newControl = new StswTextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                newControl.SetBinding(StswTextBox.BorderBrushProperty, bindingBorderBrush);
                newControl.SetBinding(StswTextBox.BorderThicknessProperty, bindingBorderThickness);
                newControl.SetBinding(StswTextBox.CornerClippingProperty, bindingCornerClipping);
                newControl.SetBinding(StswTextBox.CornerRadiusProperty, bindingCornerRadius);
                newControl.SetBinding(StswTextBox.IconProperty, bindingIcon);
                newControl.SetBinding(StswTextBox.IsReadOnlyProperty, bindingIsReadOnly);
                newControl.SetBinding(StswTextBox.PaddingProperty, bindingPadding);
                newControl.SetBinding(StswTextBox.PlaceholderProperty, bindingPlaceholder);
                newControl.SetBinding(StswTextBox.SubControlsProperty, bindingSubControls);
                newControl.SetBinding(StswTextBox.TextProperty, bindingValue);
                newControl.SetBinding(StswTextBox.HorizontalContentAlignmentProperty, bindingHorizontalContentAlignment);
                newControl.SetBinding(StswTextBox.VerticalContentAlignmentProperty, bindingVerticalContentAlignment);
                break;
            case StswAdaptiveType.Time:
                newControl = new StswTimePicker()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                newControl.SetBinding(StswTimePicker.BorderBrushProperty, bindingBorderBrush);
                newControl.SetBinding(StswTimePicker.BorderThicknessProperty, bindingBorderThickness);
                newControl.SetBinding(StswTimePicker.CornerClippingProperty, bindingCornerClipping);
                newControl.SetBinding(StswTimePicker.CornerRadiusProperty, bindingCornerRadius);
                newControl.SetBinding(StswTimePicker.FormatProperty, bindingFormat);
                newControl.SetBinding(StswTimePicker.IconProperty, bindingIcon);
                newControl.SetBinding(StswTimePicker.IsReadOnlyProperty, bindingIsReadOnly);
                newControl.SetBinding(StswTimePicker.PaddingProperty, bindingPadding);
                newControl.SetBinding(StswTimePicker.PlaceholderProperty, bindingPlaceholder);
                newControl.SetBinding(StswTimePicker.SelectedTimeProperty, bindingValue);
                newControl.SetBinding(StswTimePicker.SeparatorThicknessProperty, bindingSeparatorThickness);
                newControl.SetBinding(StswTimePicker.SubControlsProperty, bindingSubControls);
                newControl.SetBinding(StswTimePicker.HorizontalContentAlignmentProperty, bindingHorizontalContentAlignment);
                newControl.SetBinding(StswTimePicker.VerticalContentAlignmentProperty, bindingVerticalContentAlignment);
                break;
        }

        if (newControl != null)
            _contentPresenter.Content = newControl;
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
            typeof(StswAdaptiveBox)
        );

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
            typeof(StswAdaptiveBox)
        );

    /// <summary>
    /// Gets or sets the custom format string used to display the value in the control. 
    /// Applicable for types such as Date and Number.
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
            typeof(StswAdaptiveBox),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

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
            typeof(StswAdaptiveBox)
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
            typeof(StswAdaptiveBox)
        );

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
            typeof(StswAdaptiveBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the checkbox input (if applicable) supports three states (Checked, Unchecked, Indeterminate).
    /// </summary>
    public bool IsThreeState
    {
        get => (bool)GetValue(IsThreeStateProperty);
        set => SetValue(IsThreeStateProperty, value);
    }
    public static readonly DependencyProperty IsThreeStateProperty
        = DependencyProperty.Register(
            nameof(IsThreeState),
            typeof(bool),
            typeof(StswAdaptiveBox)
        );

    /// <summary>
    /// Gets or sets the collection of items used to populate a selection-based input (e.g., dropdown).
    /// Used when <see cref="Type"/> is set to List.
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
            typeof(StswAdaptiveBox)
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
            typeof(StswAdaptiveBox)
        );

    /// <summary>
    /// Gets or sets the property path used to extract values from the <see cref="ItemsSource"/>.
    /// Used when <see cref="Type"/> is set to List.
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
            typeof(StswAdaptiveBox)
        );

    /// <summary>
    /// Gets or sets the selection unit for the date picker input.
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
            typeof(StswAdaptiveBox)
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
            typeof(StswAdaptiveBox),
            new FrameworkPropertyMetadata(default(ObservableCollection<IStswSubControl>),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the type of input box to display. 
    /// If set to <see cref="StswAdaptiveType.Auto"/>, the control attempts to determine the type based on the bound value.
    /// </summary>
    public StswAdaptiveType Type
    {
        get => (StswAdaptiveType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }
    public static readonly DependencyProperty TypeProperty
        = DependencyProperty.Register(
            nameof(Type),
            typeof(StswAdaptiveType),
            typeof(StswAdaptiveBox),
            new FrameworkPropertyMetadata(default(StswAdaptiveType),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTypeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswAdaptiveBox stsw)
            return;

        if (stsw.Type == StswAdaptiveType.Auto)
        {
            if (stsw.ItemsSource != default)
                stsw.Type = StswAdaptiveType.List;
            else
            {
                /// find type based on binded property type
                if (stsw.GetBindingExpression(ValueProperty) is BindingExpression b)
                {
                    if (b.ResolvedSource?.GetType()?.GetProperty(b.ResolvedSourcePropertyName)?.PropertyType is Type t)
                    {
                        if (t.In(typeof(bool), typeof(bool?)))
                            stsw.Type = StswAdaptiveType.Check;
                        else if (t.In(typeof(DateTime), typeof(DateTime?)))
                            stsw.Type = StswAdaptiveType.Date;
                        else if (t.IsNumericType())
                            stsw.Type = StswAdaptiveType.Number;
                        else if (t.In(typeof(string)))
                            stsw.Type = StswAdaptiveType.Text;
                        else if (t.In(typeof(TimeSpan), typeof(TimeSpan?)))
                            stsw.Type = StswAdaptiveType.Time;
                    }
                }

                /// if type is still not found then try to determine type based on value
                if (stsw.Type == StswAdaptiveType.Auto)
                {
                    if (stsw.Value is bool? || bool.TryParse(stsw.Value?.ToString(), out var _))
                        stsw.Type = StswAdaptiveType.Check;
                    else if (stsw.Value is DateTime? || DateTime.TryParse(stsw.Value?.ToString(), out var _))
                        stsw.Type = StswAdaptiveType.Date;
                    else if (stsw.Value is decimal? || decimal.TryParse(stsw.Value?.ToString(), out var _)
                          || stsw.Value is double? || double.TryParse(stsw.Value?.ToString(), out var _)
                          || stsw.Value is int? || int.TryParse(stsw.Value?.ToString(), out var _))
                        stsw.Type = StswAdaptiveType.Number;
                    else if (stsw.Value is string)
                        stsw.Type = StswAdaptiveType.Text;
                    else if (stsw.Value is TimeSpan? || TimeSpan.TryParse(stsw.Value?.ToString(), out var _))
                        stsw.Type = StswAdaptiveType.Time;
                }
            }
        }
        stsw.CreateControlBasedOnType();
    }

    /// <summary>
    /// Gets or sets the value of the input box. 
    /// The type of control is determined based on the value if <see cref="Type"/> is set to Auto.
    /// </summary>
    public object? Value
    {
        get => (object?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(StswAdaptiveBox),
            new FrameworkPropertyMetadata(default(object?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null, null, false, UpdateSourceTrigger.PropertyChanged)
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
            typeof(StswAdaptiveBox),
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
            typeof(StswAdaptiveBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the input field and additional elements (if applicable).
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
            typeof(StswAdaptiveBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
