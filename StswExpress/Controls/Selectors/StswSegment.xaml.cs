﻿using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;/// <summary>
/// A segmented control that displays a collection of selectable options in a horizontal or vertical layout.
/// Supports selection binding, read-only mode, and customizable corner styling.
/// </summary>
/// <remarks>
/// When <see cref="ItemsSource"/> contains items of type <see cref="IStswSelectionItem"/>, selection is automatically bound.
/// </remarks>
public class StswSegment : ListBox, IStswCornerControl, IStswSelectionControl
{
    static StswSegment()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSegment), new FrameworkPropertyMetadata(typeof(StswSegment)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswSegment), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswSegmentItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswSegmentItem;

    #region Events & methods
    /// <summary>
    /// Called when the <see cref="ItemsSource"/> property changes.
    /// Updates the selection state and propagates changes to the selection control logic.
    /// </summary>
    /// <param name="oldValue">The previous <see cref="ItemsSource"/> collection.</param>
    /// <param name="newValue">The new <see cref="ItemsSource"/> collection.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Called when the <see cref="ItemTemplate"/> property changes.
    /// Updates the selection control logic based on the new item template.
    /// </summary>
    /// <param name="oldItemTemplate">The previous data template for items.</param>
    /// <param name="newItemTemplate">The new data template for items.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <summary>
    /// Occurs when the PreviewKeyDown event is triggered.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (!IStswSelectionControl.PreviewKeyDown(this, e)) return;
        base.OnPreviewKeyDown(e);
    }

    /// <summary>
    /// Handles selection changes within the segmented control.
    /// If the control is in read-only mode, the selection change is prevented.
    /// Otherwise, the selection state is updated accordingly.
    /// </summary>
    /// <param name="e">Provides data for the <see cref="SelectionChanged"/> event.</param>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        IStswSelectionControl.SelectionChanged(this, e.AddedItems, e.RemovedItems);
    }

    /// <summary>
    /// Prepares the specified element to display the given item.
    /// Ensures that the item container inherits the <see cref="IsReadOnly"/> property binding.
    /// </summary>
    /// <param name="element">The element used to display the specified item.</param>
    /// <param name="item">The data item to be displayed.</param>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StswSegmentItem listBoxItem)
        {
            listBoxItem.SetBinding(StswSegmentItem.IsReadOnlyProperty, new Binding(nameof(IsReadOnly))
            {
                Source = this,
                Mode = BindingMode.OneWay
            });
        }
    }
    #endregion

    #region Logic properties
    /// <inheritdoc/>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswSegment)
        );

    /// <summary>
    /// Gets or sets the orientation of the segmented control.
    /// Determines whether options are arranged horizontally or vertically.
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
            typeof(StswSegment),
            new FrameworkPropertyMetadata(default(Orientation), FrameworkPropertyMetadataOptions.AffectsArrange)
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
            typeof(StswSegment),
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
            typeof(StswSegment),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswSegment ItemsSource="{Binding Categories}" IsReadOnly="True">
    <se:StswSegmentItem Content="Option 1"/>
    <se:StswSegmentItem Content="Option 2"/>
</se:StswSegment>

*/
