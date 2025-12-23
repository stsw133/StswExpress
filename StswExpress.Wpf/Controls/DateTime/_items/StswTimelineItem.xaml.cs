using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress.Wpf;

/// <summary>
/// A container for items displayed within <see cref="StswTimeline"/>.
/// </summary>
public class StswTimelineItem : ContentControl
{
    static StswTimelineItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimelineItem), new FrameworkPropertyMetadata(typeof(StswTimelineItem)));
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets the date associated with the timeline item.
    /// </summary>
    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }
    public static readonly DependencyProperty DateProperty
        = DependencyProperty.Register(
            nameof(Date),
            typeof(DateTime),
            typeof(StswTimelineItem),
            new FrameworkPropertyMetadata(default(DateTime), FrameworkPropertyMetadataOptions.AffectsParentArrange)
        );

    /// <summary>
    /// Gets or sets the orientation inherited from the parent timeline.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswTimelineItem),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsArrange)
        );

    /// <summary>
    /// Gets or sets the popup template displayed when hovering over the item.
    /// </summary>
    public DataTemplate? PopupTemplate
    {
        get => (DataTemplate?)GetValue(PopupTemplateProperty);
        set => SetValue(PopupTemplateProperty, value);
    }
    public static readonly DependencyProperty PopupTemplateProperty
        = DependencyProperty.Register(
            nameof(PopupTemplate),
            typeof(DataTemplate),
            typeof(StswTimelineItem)
        );

    /// <summary>
    /// Gets or sets the template selector for the popup content.
    /// </summary>
    public DataTemplateSelector? PopupTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(PopupTemplateSelectorProperty);
        set => SetValue(PopupTemplateSelectorProperty, value);
    }
    public static readonly DependencyProperty PopupTemplateSelectorProperty
        = DependencyProperty.Register(
            nameof(PopupTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(StswTimelineItem)
        );

    /// <summary>
    /// Gets or sets the initial delay for showing the tooltip associated with this item.
    /// </summary>
    public int ToolTipInitialShowDelay
    {
        get => (int)GetValue(ToolTipInitialShowDelayProperty);
        set => SetValue(ToolTipInitialShowDelayProperty, value);
    }
    public static readonly DependencyProperty ToolTipInitialShowDelayProperty
        = DependencyProperty.Register(
            nameof(ToolTipInitialShowDelay),
            typeof(int),
            typeof(StswTimelineItem),
            new FrameworkPropertyMetadata(200)
        );

    /// <summary>
    /// Gets or sets the duration the tooltip stays visible for this item.
    /// </summary>
    public int ToolTipShowDuration
    {
        get => (int)GetValue(ToolTipShowDurationProperty);
        set => SetValue(ToolTipShowDurationProperty, value);
    }
    public static readonly DependencyProperty ToolTipShowDurationProperty
        = DependencyProperty.Register(
            nameof(ToolTipShowDuration),
            typeof(int),
            typeof(StswTimelineItem),
            new FrameworkPropertyMetadata(10000)
        );
    #endregion
}
