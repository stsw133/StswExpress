using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that displays a collection of items in a vertical list.
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically bind selected items.
/// </summary>
public class StswListBox : ListBox, IStswCornerControl, IStswScrollableControl
{
    static StswListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswListBox), new FrameworkPropertyMetadata(typeof(StswListBox)));
    }

    #region Events & methods
    /// <summary>
    /// Handles the event triggered when the ItemsSource property changes in the control.
    /// Checks if the ItemsSource collection contains items implementing the <see cref="IStswSelectionItem"/> interface
    /// to enable advanced selection features.
    /// </summary>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        if (ItemsSource?.GetType()?.IsListType(out var innerType) == true)
            UsesSelectionItems = innerType?.IsAssignableTo(typeof(IStswSelectionItem)) == true;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets a value indicating whether the control uses selection items that implement
    /// the <see cref="IStswSelectionItem"/> interface to enable advanced selection features.
    /// </summary>
    internal bool UsesSelectionItems
    {
        get => (bool)GetValue(UsesSelectionItemsProperty);
        set => SetValue(UsesSelectionItemsProperty, value);
    }
    public static readonly DependencyProperty UsesSelectionItemsProperty
        = DependencyProperty.Register(
            nameof(UsesSelectionItems),
            typeof(bool),
            typeof(StswListBox)
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
            typeof(StswListBox)
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
            typeof(StswListBox)
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
            typeof(StswListBox)
        );
    #endregion
}
