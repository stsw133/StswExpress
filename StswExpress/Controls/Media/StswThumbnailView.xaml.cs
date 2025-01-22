using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

    protected override DependencyObject GetContainerForItemOverride() => new StswThumbnailItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswThumbnailItem;

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
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
            typeof(StswThumbnailView),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswThumbnailView),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/// <summary>
/// 
/// </summary>
internal class StswThumbnailItem : ListBoxItem
{
    static StswThumbnailItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswThumbnailItem), new FrameworkPropertyMetadata(typeof(StswThumbnailItem)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswThumbnailItem), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (DataContext is IStswSelectionItem)
            SetBinding(IsSelectedProperty, new Binding(nameof(IStswSelectionItem.IsSelected)));
    }
    #endregion
}
