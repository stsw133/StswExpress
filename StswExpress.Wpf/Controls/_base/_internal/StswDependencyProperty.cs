using System.Windows;

namespace StswExpress.Wpf;

/// <summary>
/// Provides helper methods for registering dependency properties in WPF controls.
/// </summary>
internal static class StswDependencyProperty
{
    /// <summary>
    /// Registers a new dependency property of the specified type for the given control type, using the provided
    /// property name, default value, metadata options, and property changed callback.
    /// </summary>
    /// <remarks>This method provides a type-safe way to register dependency properties for custom controls.
    /// It is typically used within a static constructor of a control class to define new dependency properties. The
    /// returned DependencyProperty should be stored in a static readonly field for later use.</remarks>
    /// <typeparam name="TControl">The type of control that owns the dependency property. Must derive from DependencyObject.</typeparam>
    /// <typeparam name="TProperty">The type of the value stored by the dependency property.</typeparam>
    /// <param name="name">The name of the dependency property to register. Cannot be null or empty.</param>
    /// <param name="defaultValue">The default value for the dependency property. If not specified, the default value for the property type is
    /// used.</param>
    /// <param name="options">A bitwise combination of FrameworkPropertyMetadataOptions values that specify property behavior, such as whether
    /// the property supports data binding, inherits values, or affects rendering. The default is
    /// FrameworkPropertyMetadataOptions.None.</param>
    /// <param name="changed">A callback that is invoked when the property value changes, or null if no callback is required.</param>
    /// <returns>A DependencyProperty instance that represents the registered dependency property.</returns>
    public static DependencyProperty Register<TControl, TProperty>(
        string name,
        TProperty? defaultValue = default,
        FrameworkPropertyMetadataOptions options = FrameworkPropertyMetadataOptions.None,
        PropertyChangedCallback? changed = null)
        where TControl : DependencyObject
        => DependencyProperty.Register(
            name,
            typeof(TProperty),
            typeof(TControl),
            new FrameworkPropertyMetadata(defaultValue, options, changed)
        );
}
