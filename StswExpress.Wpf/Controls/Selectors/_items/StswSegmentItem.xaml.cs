using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress.Wpf;/// <summary>
/// Represents an individual item inside the <see cref="StswSegment"/>.
/// Supports selection state binding and corner customization.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSegment&gt;
///     &lt;se:StswSegmentItem Content="Tab 1"/&gt;
///     &lt;se:StswSegmentItem Content="Tab 2"/&gt;
/// &lt;/se:StswSegment&gt;
/// </code>
/// </example>
public class StswSegmentItem : ListBoxItem, IStswCornerControl
{
    static StswSegmentItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSegmentItem), new FrameworkPropertyMetadata(typeof(StswSegmentItem)));
    }

    #region Dependency properties
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
            typeof(StswSegmentItem)
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
            typeof(StswSegmentItem)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the item is in read-only mode.
    /// When set to <see langword="true"/>, the item becomes unselectable and unclickable.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswSegmentItem)
        );
    #endregion

    #region Template
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (DataContext is IStswSelectableItem)
            SetBinding(IsSelectedProperty, new Binding(nameof(IStswSelectableItem.IsSelected)));
    }
    #endregion
}
