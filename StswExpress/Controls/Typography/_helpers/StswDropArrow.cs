using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Provides attached properties to control the behavior and appearance of a drop-down arrow.
/// Enables customization of the arrow's visibility, rotation, and geometry.
/// </summary>
public static class StswDropArrow
{
    /// <summary>
    /// Gets or sets the geometry data for the drop-down arrow.
    /// Defines the vector shape of the arrow.
    /// </summary>
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.RegisterAttached(
            nameof(DataProperty)[..^8],
            typeof(Geometry),
            typeof(StswDropArrow),
            new PropertyMetadata(default(Geometry), OnPropertyChanged)
        );
    public static void SetData(DependencyObject d, Geometry value) => d.SetValue(DataProperty, value);

    /// <summary>
    /// Gets or sets a value indicating whether the drop-down arrow is rotated.
    /// When set to <see langword="true"/>, the arrow is rotated to indicate an expanded state.
    /// </summary>
    public static readonly DependencyProperty IsRotatedProperty
        = DependencyProperty.RegisterAttached(
            nameof(IsRotatedProperty)[..^8],
            typeof(bool),
            typeof(StswDropArrow),
            new PropertyMetadata(default(bool), OnPropertyChanged)
        );
    public static void SetIsRotated(DependencyObject d, bool value) => d.SetValue(IsRotatedProperty, value);

    /// <summary>
    /// Tracks whether change callbacks have already been registered for a given control.
    /// </summary>
    private static readonly DependencyProperty IsTrackingProperty
        = DependencyProperty.RegisterAttached(
            nameof(IsTrackingProperty)[..^8],
            typeof(bool),
            typeof(StswDropArrow),
            new PropertyMetadata(false)
        );

    /// <summary>
    /// Identifies the <see cref="Visibility"/> attached property.
    /// When set to <see cref="Visibility.Collapsed"/>, the drop-down arrow is hidden.
    /// </summary>
    public static readonly DependencyProperty VisibilityProperty
        = DependencyProperty.RegisterAttached(
            nameof(VisibilityProperty)[..^8],
            typeof(Visibility),
            typeof(StswDropArrow),
            new PropertyMetadata(default(Visibility), OnPropertyChanged)
        );
    public static void SetVisibility(DependencyObject d, Visibility value) => d.SetValue(VisibilityProperty, value);

    /// <summary>
    /// Applies changes to the drop-down arrow when a property value changes.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="e">The event arguments containing the changed property.</param>
    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Control control)
            EnsureTemplate(control);
    }

    /// <summary>
    /// Ensures that the control has a template before applying drop-arrow properties.
    /// </summary>
    /// <param name="control">The control instance to check.</param>
    private static void EnsureTemplate(Control control)
    {
        if (control.Template == null)
        {
            if (!control.IsLoaded)
            {
                control.Loaded -= Control_Loaded;
                control.Loaded += Control_Loaded;
            }
            return;
        }

        control.ApplyTemplate();
        ApplyDropArrowProperties(control);

        if (!(bool)control.GetValue(IsTrackingProperty))
        {
            TrackPropertyChanges(control);
            control.SetValue(IsTrackingProperty, true);
        }
    }

    /// <summary>
    /// Applies drop-arrow properties such as geometry, rotation, and visibility to the target control.
    /// </summary>
    /// <param name="control">The control to update.</param>
    private static void ApplyDropArrowProperties(Control control)
    {
        if (control.Template?.FindName("OPT_DropArrow", control) is StswIcon dropArrow)
        {
            ApplyBindingOrValue(control, dropArrow, DataProperty, StswIcon.DataProperty);
            ApplyBindingOrValue(control, dropArrow, IsRotatedProperty, StswIcon.IsRotatedProperty);
            ApplyBindingOrValue(control, dropArrow, VisibilityProperty, UIElement.VisibilityProperty);
        }
    }

    /// <summary>
    /// Applies either a binding or a direct value from the source control to the drop-arrow element.
    /// </summary>
    /// <param name="source">The control hosting the attached property.</param>
    /// <param name="target">The drop-arrow element receiving the value.</param>
    /// <param name="attachedProperty">The attached property storing the value or binding.</param>
    /// <param name="targetProperty">The dependency property on the target element to update.</param>
    private static void ApplyBindingOrValue(DependencyObject source, DependencyObject target, DependencyProperty attachedProperty, DependencyProperty targetProperty)
    {
        var valueSource = DependencyPropertyHelper.GetValueSource(source, attachedProperty);
        if (valueSource.BaseValueSource == BaseValueSource.Default && !valueSource.IsExpression)
        {
            target.ClearValue(targetProperty);
            return;
        }

        var value = source.GetValue(attachedProperty);
        if (value == DependencyProperty.UnsetValue)
        {
            target.ClearValue(targetProperty);
            return;
        }

        target.SetValue(targetProperty, value);
    }

    /// <summary>
    /// Handles the <see cref="FrameworkElement.Loaded"/> event to apply drop-arrow settings once the control is fully loaded.
    /// </summary>
    /// <param name="sender">The control that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private static void Control_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Control control)
        {
            control.Loaded -= Control_Loaded;
            EnsureTemplate(control);
        }
    }

    /// <summary>
    /// Tracks changes to drop-arrow-related properties and updates the control accordingly.
    /// </summary>
    /// <param name="control">The control to monitor for property changes.</param>
    private static void TrackPropertyChanges(Control control)
    {
        DependencyPropertyDescriptor.FromProperty(DataProperty, typeof(StswDropArrow))?.AddValueChanged(control, (s, e) => ApplyDropArrowProperties(control));
        DependencyPropertyDescriptor.FromProperty(IsRotatedProperty, typeof(StswDropArrow))?.AddValueChanged(control, (s, e) => ApplyDropArrowProperties(control));
        DependencyPropertyDescriptor.FromProperty(VisibilityProperty, typeof(StswDropArrow))?.AddValueChanged(control, (s, e) => ApplyDropArrowProperties(control));
    }
}
