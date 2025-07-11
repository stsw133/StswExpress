using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;/// <summary>
/// Represents an individual item inside the <see cref="StswTreeView"/>.
/// Supports selection state binding, hierarchical data, read-only mode, and corner customization.
/// </summary>
/// <remarks>
/// When <see cref="DataContext"/> implements <see cref="IStswSelectionItem"/>, selection binding is automatically applied.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTreeView&gt;
///     &lt;se:StswTreeViewItem Header="Root"&gt;
///         &lt;se:StswTreeViewItem Header="Child 1"/&gt;
///         &lt;se:StswTreeViewItem Header="Child 2"/&gt;
///     &lt;/se:StswTreeViewItem&gt;
/// &lt;/se:StswTreeView&gt;
/// </code>
/// </example>
[StswInfo("0.14.0")]
public class StswTreeViewItem : TreeViewItem
{
    static StswTreeViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTreeViewItem), new FrameworkPropertyMetadata(typeof(StswTreeViewItem)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswTreeViewItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTreeViewItem;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (DataContext is IStswSelectionItem)
            SetBinding(IsSelectedProperty, new Binding(nameof(IStswSelectionItem.IsSelected)));
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StswTreeViewItem listBoxItem)
        {
            listBoxItem.SetBinding(StswTreeViewItem.IsReadOnlyProperty, new Binding(nameof(IsReadOnly))
            {
                Source = this,
                Mode = BindingMode.OneWay
            });
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the item is in read-only mode.
    /// When set to <see langword="true"/>, the item cannot be selected or interacted with.
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
            typeof(StswTreeViewItem)
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
            typeof(StswTreeViewItem),
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
            typeof(StswTreeViewItem),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
