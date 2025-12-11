using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress.Wpf;

/// <summary>
/// A XAML markup extension that retrieves the <see cref="Color"/> from a <see cref="SolidColorBrush"/> resource
/// using a specified resource key. This extension enables seamless conversion from <see cref="SolidColorBrush"/>
/// to <see cref="Color"/> in XAML.
/// </summary>
/// <remarks>
/// Useful for scenarios where a color is needed directly instead of a brush, such as bindings to properties
/// expecting a <see cref="Color"/> instead of a <see cref="SolidColorBrush"/>.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;LinearGradientBrush&gt;
///     &lt;GradientStop Color="{se:StswDynamicColor GradientBrushResource}" Offset="0.5"/&gt;
/// &lt;/LinearGradientBrush&gt;
/// </code>
/// </example>
public class StswDynamicColorExtension(string resourceKey) : MarkupExtension
{
    /// <summary>
    /// Gets or sets the resource key used to locate the <see cref="SolidColorBrush"/>.
    /// </summary>
    public string ResourceKey { get; set; } = resourceKey;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Application.Current?.TryFindResource(ResourceKey) is not SolidColorBrush brush)
            throw new InvalidOperationException($"Resource with key '{ResourceKey}' not found or not a {nameof(SolidColorBrush)}.");

        return brush.Color;
    }
}
