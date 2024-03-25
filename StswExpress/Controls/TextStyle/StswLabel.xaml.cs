using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control functioning as label.
/// </summary>
public class StswLabel : Label, IStswCornerControl
{
    public StswLabel()
    {
        DependencyPropertyDescriptor.FromProperty(IsTruncationAllowedProperty, typeof(StswLabel)).AddValueChanged(this, (s, e) => UpdateContentTruncation());
    }
    static StswLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLabel), new FrameworkPropertyMetadata(typeof(StswLabel)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateContentTruncation();
    }

    /// <summary>
    /// Occurs when the content of the label is changed.
    /// </summary>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateContentTruncation();
    }

    /// <summary>
    /// Occurs when the render size of the label is changed.
    /// </summary>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateContentTruncation();
    }

    /// <summary>
    /// Updates the content truncation status.
    /// </summary>
    private void UpdateContentTruncation()
    {
        if (!IsTruncationAllowed)
        {
            IsContentTruncated = false;
            return;
        }

        var desiredSize = MeasureContentDesiredSize(Content);
        var availableSize = new Size(ActualWidth, ActualHeight);

        IsContentTruncated = desiredSize.Width > availableSize.Width || desiredSize.Height > availableSize.Height;
    }

    /// <summary>
    /// Measures the desired size of the content for truncation.
    /// </summary>
    private Size MeasureContentDesiredSize(object content)
    {
        if (content is UIElement contentElement)
        {
            contentElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return contentElement.DesiredSize;
        }
        else if (content is string textContent)
        {
            var formattedText = new FormattedText(textContent, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            return new Size(formattedText.Width, formattedText.Height);
        }
        else return new Size(0, 0);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets a value indicating whether the content is truncated.
    /// </summary>
    public bool IsContentTruncated
    {
        get => (bool)GetValue(IsContentTruncatedProperty);
        internal set => SetValue(IsContentTruncatedPropertyKey, value);
    }
    public static readonly DependencyProperty? IsContentTruncatedProperty = IsContentTruncatedPropertyKey?.DependencyProperty;
    internal static readonly DependencyPropertyKey IsContentTruncatedPropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(IsContentTruncated),
            typeof(bool),
            typeof(StswLabel),
            new PropertyMetadata(false)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the truncation button is visible.
    /// </summary>
    public bool IsTruncationAllowed
    {
        get => (bool)GetValue(IsTruncationAllowedProperty);
        set => SetValue(IsTruncationAllowedProperty, value);
    }
    public static readonly DependencyProperty IsTruncationAllowedProperty
        = DependencyProperty.Register(
            nameof(IsTruncationAllowed),
            typeof(bool),
            typeof(StswLabel)
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
            typeof(StswLabel)
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
            typeof(StswLabel)
        );
    #endregion
}
