using System.Windows;
using System.Windows.Markup;

namespace StswExpress.Wpf;

/// <summary>
/// Represents a button control with an expandable drop-down area.
/// The main button content is set using the <see cref="Header"/> property,
/// while additional elements can be placed inside the drop-down menu.
/// Supports optional auto-closing, customizable corner rounding, and styling enhancements.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSplitButton&gt;
///     &lt;se:StswSplitButton.Header&gt;
///         &lt;se:TextBox Text="Enter text..."/&gt;
///     &lt;/se:StswSplitButton.Header&gt;
///     &lt;se:Button Command="{Binding ClearTextCommand}" Content="Clear"/&gt;
///     &lt;se:Button Command="{Binding SubmitTextCommand}" Content="Submit"/&gt;
/// &lt;/se:StswSplitButton&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Items))]
public class StswSplitButton : StswDropButton
{
    static StswSplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSplitButton), new FrameworkPropertyMetadata(typeof(StswSplitButton)));
    }

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the separator between the main button and the drop-down arrow.
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
            typeof(StswSplitButton),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
