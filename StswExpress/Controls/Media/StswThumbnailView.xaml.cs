using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// 
/// </summary>
internal class StswThumbnailView : ListBox, IStswCornerControl
{
    static StswThumbnailView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswThumbnailView), new FrameworkPropertyMetadata(typeof(StswThumbnailView)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswThumbnailView), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswThumbnailViewItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswThumbnailViewItem;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the stretch behavior of the control.
    /// </summary>
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }
    public static readonly DependencyProperty StretchProperty
        = DependencyProperty.Register(
            nameof(Stretch),
            typeof(Stretch),
            typeof(StswThumbnailView)
        );

    /// <summary>
    /// Gets or sets the stretch direction of the control.
    /// </summary>
    public StretchDirection StretchDirection
    {
        get => (StretchDirection)GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }
    public static readonly DependencyProperty StretchDirectionProperty
        = DependencyProperty.Register(
            nameof(StretchDirection),
            typeof(StretchDirection),
            typeof(StswThumbnailView)
        );
    #endregion

    #region Style properties
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
            typeof(StswThumbnailView),
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
            typeof(StswThumbnailView),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
