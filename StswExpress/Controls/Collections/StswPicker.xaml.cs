using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that displays a collection of items in a vertical list.
/// </summary>
[Obsolete("Not fully implemented and heavily bugged. Please do not use!", true)]
public class StswPicker : ListBox, IStswCornerControl
{
    static StswPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPicker), new FrameworkPropertyMetadata(typeof(StswPicker)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);

        if (Items == null || Items.Count == 0)
            return;

        var index = SelectedIndex > -1 ? SelectedIndex : 0;
        ItemsVisible = new List<object>();
        while (ItemsVisible.Count < (ItemsNumber / 2 * 2 + 1))
        {
            if (ItemsSource != null && ItemsSource is IList source)
                ItemsVisible.Add(source[StswFn.ShiftIndexBy(index, source.Count, ItemsVisible.Count - (ItemsNumber / 2), true)]);
            else
                ItemsVisible.Add(Items[StswFn.ShiftIndexBy(index, Items.Count, ItemsVisible.Count - ItemsNumber / 2, true)]);
        }
    }

    /// <summary>
    /// Handles the event triggered when the SelectedItem property changes in the control.
    /// </summary>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (Items == null || Items.Count == 0)
            return;

        var index = SelectedIndex > -1 ? SelectedIndex : 0;
        if (index == ItemsVisible?.IndexOf(ItemsNumber / 2 * 2 + 1))
            return;

        ItemsVisible = new List<object>();
        while (ItemsVisible.Count < (ItemsNumber / 2 * 2 + 1))
        {
            if (ItemsSource != null && ItemsSource is IList source)
                ItemsVisible.Add(source[StswFn.ShiftIndexBy(index, source.Count, ItemsVisible.Count - (ItemsNumber / 2), true)]);
            else
                ItemsVisible.Add(Items[StswFn.ShiftIndexBy(index, Items.Count, ItemsVisible.Count - ItemsNumber / 2, true)]);
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// 
    /// </summary>
    public int ItemsNumber
    {
        get => (int)GetValue(ItemsNumberProperty);
        set => SetValue(ItemsNumberProperty, value);
    }
    public static readonly DependencyProperty ItemsNumberProperty
        = DependencyProperty.Register(
            nameof(ItemsNumber),
            typeof(int),
            typeof(StswPicker)
        );

    /// <summary>
    /// 
    /// </summary>
    public List<object> ItemsVisible
    {
        get => (List<object>)GetValue(ItemsVisibleProperty);
        internal set => SetValue(ItemsVisibleProperty, value);
    }
    public static readonly DependencyProperty ItemsVisibleProperty
        = DependencyProperty.Register(
            nameof(ItemsVisible),
            typeof(List<object>),
            typeof(StswPicker)
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
            typeof(StswPicker)
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
            typeof(StswPicker)
        );
    #endregion
}
