using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Provides a way to dynamically retrieve the <see cref="Color"/> from a <see cref="SolidColorBrush"/> resource
/// using a specified resource key. This extension allows seamless conversion from <see cref="SolidColorBrush"/>
/// to <see cref="Color"/> in XAML.
/// </summary>
public class StswDynamicColorExtension : MarkupExtension
{
    public string ResourceKey { get; set; }

    public StswDynamicColorExtension(string resourceKey)
    {
        ResourceKey = resourceKey;
    }

    /// <summary>
    /// Returns the <see cref="Color"/> from the <see cref="SolidColorBrush"/> resource identified by the specified resource key.
    /// </summary>
    /// <param name="serviceProvider">An object that can provide services for the markup extension. This parameter is typically ignored.</param>
    /// <returns>The <see cref="Color"/> extracted from the specified <see cref="SolidColorBrush"/> resource.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <see cref="ResourceKey"/> is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the resource with the specified <see cref="ResourceKey"/> cannot be found or is not a <see cref="SolidColorBrush"/>.
    /// </exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(ResourceKey))
            throw new ArgumentNullException(nameof(ResourceKey));

        if (Application.Current.TryFindResource(ResourceKey) is not SolidColorBrush brush)
            throw new InvalidOperationException($"Resource with key '{ResourceKey}' not found or not a {nameof(SolidColorBrush)}.");

        return brush.Color;
    }
}
