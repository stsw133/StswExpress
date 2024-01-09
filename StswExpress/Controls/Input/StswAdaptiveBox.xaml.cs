using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A control used for automatically select input box based on its value type.
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically bind selected items.
/// </summary>
[ContentProperty(nameof(Value))]
public class StswAdaptiveBox : Control, IStswCornerControl
{
    public StswAdaptiveBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswAdaptiveBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswAdaptiveBox), new FrameworkPropertyMetadata(typeof(StswAdaptiveBox)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        OnTypeChanged(this, new DependencyPropertyChangedEventArgs());
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
            typeof(StswAdaptiveBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the control is in read-only mode.
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
            typeof(StswAdaptiveBox)
        );
    
    /// <summary>
    /// 
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
            typeof(StswAdaptiveBox)
        );

    /// <summary>
    /// Gets or sets the placeholder text to display in the box when no value is provided.
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
            typeof(StswAdaptiveBox)
        );

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
            typeof(StswAdaptiveBox)
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
            typeof(StswAdaptiveBox)
        );

    /// <summary>
    /// Gets or sets the type of box to be applied.
    /// </summary>
    public StswAdaptiveType? Type
    {
        get => (StswAdaptiveType?)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }
    public static readonly DependencyProperty TypeProperty
        = DependencyProperty.Register(
            nameof(Type),
            typeof(StswAdaptiveType?),
            typeof(StswAdaptiveBox),
            new FrameworkPropertyMetadata(default(StswAdaptiveType?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTypeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswAdaptiveBox stsw)
        {
            if (stsw.Type == null)
            {
                if (stsw.ItemsSource != default)
                    stsw.Type = StswAdaptiveType.List;
                else
                {
                    /// find type based on binded property type
                    if (stsw.GetBindingExpression(ValueProperty) is BindingExpression b and not null)
                    {
                        if (b.ResolvedSource?.GetType()?.GetProperty(b.ResolvedSourcePropertyName)?.PropertyType is Type t and not null)
                        {
                            if (t.In(typeof(bool), typeof(bool?)))
                                stsw.Type = StswAdaptiveType.Check;
                            else if (t.In(typeof(DateTime), typeof(DateTime?)))
                                stsw.Type = StswAdaptiveType.Date;
                            else if (t.IsNumericType())
                                stsw.Type = StswAdaptiveType.Number;
                            else if (t.In(typeof(string)))
                                stsw.Type = StswAdaptiveType.Text;
                        }
                    }

                    /// if type is still not found then try to determine type based on value
                    if (stsw.Type == null)
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
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the first value used in filtering.
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
            typeof(StswAdaptiveBox)
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
            typeof(StswAdaptiveBox)
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
            typeof(StswAdaptiveBox)
        );
    #endregion
}
