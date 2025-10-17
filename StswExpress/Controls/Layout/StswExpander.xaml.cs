using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// A collapsible panel control that displays a header and allows the user to expand or collapse its content.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswExpander Header="Details" IsExpanded="True"&gt;
///     &lt;TextBlock Text="Here are additional details..."/&gt;
/// &lt;/se:StswExpander&gt;
/// </code>
/// </example>
[StswPlannedChanges(StswPlannedChanges.VisualChanges)]
public class StswExpander : Expander, IStswCornerControl
{
    static StswExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswExpander), new FrameworkPropertyMetadata(typeof(StswExpander)));
        StswThickness.OverrideBaseBorderThickness<StswExpander>(getExt: c => c.BorderThickness, setExt: (c, st) => c.BorderThickness = st);
    }

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
            typeof(StswExpander),
            new FrameworkPropertyMetadata(default(StswThickness),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                StswThickness.CreateExtendedChangedCallback<StswExpander>((c, th) => c.SetCurrentValue(Control.BorderThicknessProperty, th)))
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
            typeof(StswExpander),
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
            typeof(StswExpander),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
