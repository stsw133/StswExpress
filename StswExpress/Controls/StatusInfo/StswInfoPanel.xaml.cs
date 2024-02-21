using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a panel control that displays information bars in a scrollable list, providing optional functionality for a close button.
/// </summary>
public class StswInfoPanel : ListBox, IStswCornerControl, IStswScrollableControl
{
    static StswInfoPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswInfoPanel), new FrameworkPropertyMetadata(typeof(StswInfoPanel)));
    }

    #region Events & methods
    /// <summary>
    /// Handles changes in the items collection and scrolls to the end if new items are added.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (e.NewItems?.Count > 0 && GetTemplateChild("PART_ScrollViewer") is ScrollViewer scrollViewer)
            scrollViewer?.ScrollToEnd();
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets a value indicating whether the items are closable and has a close button.
    /// </summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }
    public static readonly DependencyProperty IsClosableProperty
        = DependencyProperty.Register(
            nameof(IsClosable),
            typeof(bool),
            typeof(StswInfoPanel)
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
            typeof(StswInfoPanel)
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
            typeof(StswInfoPanel)
        );

    /// <summary>
    /// Gets or sets the data model for properties of the scroll viewer associated with the control.
    /// The <see cref="StswScrollViewerModel"/> class provides customization options for the appearance and behavior of the scroll viewer.
    /// </summary>
    public StswScrollViewerModel ScrollViewer
    {
        get => (StswScrollViewerModel)GetValue(ScrollViewerProperty);
        set => SetValue(ScrollViewerProperty, value);
    }
    public static readonly DependencyProperty ScrollViewerProperty
        = DependencyProperty.Register(
            nameof(ScrollViewer),
            typeof(StswScrollViewerModel),
            typeof(StswInfoPanel)
        );
    #endregion
}
