using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

public class StswLabel : Label
{
    static StswLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLabel), new FrameworkPropertyMetadata(typeof(StswLabel)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UpdateContentTruncation();
    }

    /// OnContentChanged
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        UpdateContentTruncation();
    }

    /// OnRenderSizeChanged
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        UpdateContentTruncation();
    }

    /// UpdateContentTruncation
    private void UpdateContentTruncation()
    {
        if (!IsTruncButtonVisible)
        {
            IsContentTruncated = false;
            return;
        }

        var desiredSize = MeasureContentDesiredSize(Content);
        var availableSize = new Size(ActualWidth, ActualHeight);

        bool isContentTruncated = desiredSize.Width > availableSize.Width || desiredSize.Height > availableSize.Height;

        IsContentTruncated = isContentTruncated;
    }

    /// MeasureContentDesiredSize
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
        else
        {
            return new Size(0, 0);
        }
    }
    #endregion

    #region Main properties
    /// IsContentTruncated
    public static readonly DependencyProperty IsContentTruncatedProperty = IsContentTruncatedPropertyKey?.DependencyProperty;
    private static readonly DependencyPropertyKey IsContentTruncatedPropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(IsContentTruncated),
            typeof(bool),
            typeof(StswLabel),
            new PropertyMetadata(false)
        );
    public bool IsContentTruncated
    {
        get => (bool)GetValue(IsContentTruncatedProperty);
        internal set => SetValue(IsContentTruncatedPropertyKey, value);
    }

    /// IsTruncButtonVisible
    public static readonly DependencyProperty IsTruncButtonVisibleProperty
        = DependencyProperty.Register(
            nameof(IsTruncButtonVisible),
            typeof(bool),
            typeof(StswLabel)
        );
    public bool IsTruncButtonVisible
    {
        get => (bool)GetValue(IsTruncButtonVisibleProperty);
        set => SetValue(IsTruncButtonVisibleProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswLabel)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
