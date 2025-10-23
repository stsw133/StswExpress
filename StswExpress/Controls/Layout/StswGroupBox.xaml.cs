using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// A container control that groups related UI elements under a common header.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswGroupBox Header="Settings"&gt;
///     &lt;CheckBox Content="Enable dark mode"/&gt;
/// &lt;/se:StswGroupBox&gt;
/// </code>
/// </example>
public class StswGroupBox : GroupBox, IStswCornerControl
{
    static StswGroupBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswGroupBox), new FrameworkPropertyMetadata(typeof(StswGroupBox)));
        //StswControl.OverrideBaseBorderThickness<StswGroupBox>(getExt: c => c.BorderThickness, setExt: (c, st) => c.BorderThickness = st);
    }

    #region Style properties
    /*
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
            typeof(StswGroupBox),
            new FrameworkPropertyMetadata(default(StswThickness),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                StswControl.CreateExtendedChangedCallback<StswGroupBox>((c, th) => c.SetCurrentValue(Control.BorderThicknessProperty, th)))
        );
    */

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
            typeof(StswGroupBox),
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
            typeof(StswGroupBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the header and the content.
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
            typeof(StswGroupBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
