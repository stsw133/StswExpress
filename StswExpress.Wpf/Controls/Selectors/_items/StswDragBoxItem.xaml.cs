using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress.Wpf;

/// <summary>
/// Represents an individual item inside the <see cref="StswDragBox"/>.
/// Supports selection state binding and corner customization.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDragBox&gt;
///     &lt;se:StswDragBoxItem Content="Item A"/&gt;
///     &lt;se:StswDragBoxItem Content="Item B"/&gt;
/// &lt;/se:StswDragBox&gt;
/// </code>
/// </example>
public class StswDragBoxItem : ListBoxItem, IStswCornerControl
{
    static StswDragBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDragBoxItem), new FrameworkPropertyMetadata(typeof(StswDragBoxItem)));
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
            typeof(StswDragBoxItem)
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
            typeof(StswDragBoxItem)
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
            typeof(StswDragBoxItem)
        );
    #endregion

    #region Template
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (DataContext?.GetType()?.IsAssignableTo(typeof(IStswSelectableItem)) == true)
            SetBinding(IsSelectedProperty, new Binding(nameof(IStswSelectableItem.IsSelected)));
    }
    #endregion
}
