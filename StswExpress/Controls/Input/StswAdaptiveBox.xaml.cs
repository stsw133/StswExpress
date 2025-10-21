using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool _autoTypeEnabled;
    private bool _isUpdatingTypeFromAuto;

    public StswAdaptiveBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswAdaptiveBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswAdaptiveBox), new FrameworkPropertyMetadata(typeof(StswAdaptiveBox)));
        StswControl.OverrideBaseBorderThickness<StswAdaptiveBox>(getExt: c => c.BorderThickness, setExt: (c, st) => c.BorderThickness = st);
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
                newControl.SetBinding(StswTimePicker.SubControlsProperty, bindingSubControls);
                newControl.SetBinding(StswTimePicker.HorizontalContentAlignmentProperty, bindingHorizontalContentAlignment);
                newControl.SetBinding(StswTimePicker.VerticalContentAlignmentProperty, bindingVerticalContentAlignment);
                break;
        }

        if (newControl != null)
            _contentPresenter.Content = newControl;
    }
    #endregion

    #region Auto type logic
    /// <summary>
    /// Attempts to update the <see cref="Type"/> property based on the inferred type when auto-detection is enabled.
    /// </summary>
    private void TryUpdateTypeFromAuto()
    {
        if (!_autoTypeEnabled)
            return;

        var inferredType = InferAutoType();
        if (inferredType == StswAdaptiveType.Auto)
        {
            if (Type == StswAdaptiveType.Auto)
                SetTypeFromAuto(StswAdaptiveType.Text);
            return;
        }

        if (Type != inferredType)
            SetTypeFromAuto(inferredType);
    }

    /// <summary>
    /// Sets the Type property from an auto-inferred value, avoiding recursive updates.
    /// </summary>
    /// <param name="targetType">The target <see cref="StswAdaptiveType"/> to set.</param>
    private void SetTypeFromAuto(StswAdaptiveType targetType)
    {
        if (Type == targetType)
            return;

        _isUpdatingTypeFromAuto = true;
        try
        {
            Type = targetType;
        }
        finally
        {
            _isUpdatingTypeFromAuto = false;
        }
    }

    /// <summary>
    /// Infers the appropriate adaptive type based on the current state of the control.
    /// </summary>
    /// <returns>The inferred <see cref="StswAdaptiveType"/>.</returns>
    private StswAdaptiveType InferAutoType()
    {
        if (ItemsSource != null)
            return StswAdaptiveType.List;

        var bindingType = GetBoundValueType();
        var inferred = ResolveTypeFromType(bindingType);
        if (inferred != StswAdaptiveType.Auto)
            return inferred;

        inferred = ResolveTypeFromType(Value?.GetType());
        if (inferred != StswAdaptiveType.Auto)
            return inferred;

        return ResolveTypeFromValueContent(Value);
    }

    /// <summary>
    /// Gets the type of the property bound to the Value property.
    /// </summary>
    /// <returns>The resolved property type if found; otherwise, <see langword="null"/>.</returns>
    private Type? GetBoundValueType()
    {
        if (GetBindingExpression(ValueProperty) is BindingExpression binding)
            return ResolveBindingPropertyType(binding);

        return null;
    }

    /// <summary>
    /// Resolves the type of the property bound to the specified binding expression.
    /// </summary>
    /// <param name="binding">The binding expression to analyze.</param>
    /// <returns>The resolved property type if found; otherwise, <see langword="null"/>.</returns>
    private static Type? ResolveBindingPropertyType(BindingExpression binding)
    {
        if (binding.ResolvedSource != null && binding.ResolvedSourcePropertyName is string propertyName)
            return ResolvePropertyType(binding.ResolvedSource.GetType(), propertyName);

        if (binding.DataItem != null)
        {
            var path = binding.ParentBinding?.Path?.Path;
            return ResolvePropertyType(binding.DataItem.GetType(), path);
        }

        return null;
    }

    /// <summary>
    /// Resolves the type of a property given its path on a specified type.
    /// </summary>
    /// <param name="type">The type to analyze.</param>
    /// <param name="path">The property path, which can include nested properties separated by dots.</param>
    /// <returns>The resolved property type if found; otherwise, <see langword="null"/>.</returns>
    private static Type? ResolvePropertyType(Type type, string? path)
    {
        if (type == null || string.IsNullOrWhiteSpace(path))
            return null;

        var currentType = type;
        var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            var propertyName = segment;
            var indexerIndex = propertyName.IndexOf('[');
            if (indexerIndex >= 0)
                propertyName = propertyName[..indexerIndex];

            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            var property = currentType.GetProperty(propertyName);
            if (property == null)
                return null;

            currentType = property.PropertyType;

            if (indexerIndex >= 0)
            {
                var elementType = GetEnumerableElementType(currentType);
                if (elementType != null)
                    currentType = elementType;
            }
        }

        return currentType;
    }

    /// <summary>
    /// Gets the element type of an enumerable type (e.g., array, list).
    /// </summary>
    /// <param name="type">The enumerable type to analyze.</param>
    /// <returns>The element type if found; otherwise, <see langword="null"/>.</returns>
    private static Type? GetEnumerableElementType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType)
        {
            var args = type.GetGenericArguments();
            if (args.Length > 0)
                return args[0];
        }

        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var args = iface.GetGenericArguments();
                if (args.Length > 0)
                    return args[0];
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves the adaptive type based on the provided .NET type.
    /// </summary>
    /// <param name="type">The .NET type to analyze.</param>
    /// <returns>The inferred <see cref="StswAdaptiveType"/>.</returns>
    private static StswAdaptiveType ResolveTypeFromType(Type? type)
    {
        if (type == null)
            return StswAdaptiveType.Auto;

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string) || type == typeof(char))
            return StswAdaptiveType.Text;
        if (type == typeof(bool))
            return StswAdaptiveType.Check;
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly))
            return StswAdaptiveType.Date;
        if (type == typeof(TimeSpan) || type == typeof(TimeOnly))
            return StswAdaptiveType.Time;
        if (type.IsEnum)
            return StswAdaptiveType.List;
        if (type.IsNumericType())
            return StswAdaptiveType.Number;

        return StswAdaptiveType.Auto;
    }

    /// <summary>
    /// Resolves the adaptive type based on the actual content of the provided value.
    /// </summary>
    /// <param name="value">The value to analyze.</param>
    /// <returns>The inferred <see cref="StswAdaptiveType"/>.</returns>
    private static StswAdaptiveType ResolveTypeFromValueContent(object? value)
    {
        if (value == null)
            return StswAdaptiveType.Auto;

        if (value is Enum)
            return StswAdaptiveType.List;

        var text = value.ToString();

        if (value is bool || bool.TryParse(text, out _))
            return StswAdaptiveType.Check;

        if (value is DateTime || DateTime.TryParse(text, out _))
            return StswAdaptiveType.Date;

        if (value is DateOnly || DateOnly.TryParse(text, out _))
            return StswAdaptiveType.Date;

        if (value is TimeOnly || TimeOnly.TryParse(text, out _))
            return StswAdaptiveType.Time;

        if (value is TimeSpan || TimeSpan.TryParse(text, out _))
            return StswAdaptiveType.Time;

        if (value is decimal || decimal.TryParse(text, out _)
            || value is double || double.TryParse(text, out _)
            || value is float || float.TryParse(text, out _)
            || value is long || long.TryParse(text, out _)
            || value is int || int.TryParse(text, out _)
            || value is short || short.TryParse(text, out _)
            || value is byte || byte.TryParse(text, out _))
            return StswAdaptiveType.Number;

        if (value is string)
            return StswAdaptiveType.Text;

        return StswAdaptiveType.Auto;
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
            typeof(StswAdaptiveBox),
            new PropertyMetadata(default(IList), OnItemsSourceChanged)
        );
    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswAdaptiveBox stsw)
            return;

        stsw.TryUpdateTypeFromAuto();
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
    public static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswAdaptiveBox stsw)
            return;

        if (stsw.Type == StswAdaptiveType.Auto)
        {
            stsw._autoTypeEnabled = true;
            stsw.TryUpdateTypeFromAuto();
            return;
        }

        if (!stsw._isUpdatingTypeFromAuto)
            stsw._autoTypeEnabled = false;

        stsw.CreateControlBasedOnType();






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
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswAdaptiveBox stsw)
            stsw.TryUpdateTypeFromAuto();
    }
    #endregion

    #region Style properties
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
            typeof(StswAdaptiveBox),
            new FrameworkPropertyMetadata(default(StswThickness),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                StswControl.CreateExtendedChangedCallback<StswAdaptiveBox>((c, th) => c.SetCurrentValue(Control.BorderThicknessProperty, th)))
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
    #endregion
}
