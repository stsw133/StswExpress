﻿using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress;
/// <summary>
/// Represents a control that displays a collection of options in a horizontal or vertical list.
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically bind selected items.
/// </summary>
public class StswSegment : ListBox, IStswCornerControl, IStswSelectionControl
{
    static StswSegment()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSegment), new FrameworkPropertyMetadata(typeof(StswSegment)));
    }

    #region Events & methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (StswSettings.Default.EnableAnimations)
        {
            if (e.AddedItems != null)
                foreach (var selectedItem in e.AddedItems)
                    if (ItemContainerGenerator.ContainerFromItem(selectedItem) is ListBoxItem item && item.Template.FindName("OPT_Border", item) is Border border)
                        AnimateSelectionChange(border, true);

            if (e.RemovedItems != null)
                foreach (var unselectedItem in e.RemovedItems)
                    if (ItemContainerGenerator.ContainerFromItem(unselectedItem) is ListBoxItem item && item.Template.FindName("OPT_Border", item) is Border border)
                        AnimateSelectionChange(border, false);
        }
    }

    /// <summary>
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Occurs when the ItemTemplate property value changes.
    /// </summary>
    /// <param name="oldItemTemplate">The old value of the ItemTemplate property.</param>
    /// <param name="newItemTemplate">The new value of the ItemTemplate property.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the orientation of the control.
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

    /// <summary>
    /// Gets or sets a value indicating whether the control uses selection items that implement
    /// the <see cref="IStswSelectionItem"/> interface to enable advanced selection features.
    /// </summary>
    public bool UsesSelectionItems
    {
        get => (bool)GetValue(UsesSelectionItemsProperty);
        set => SetValue(UsesSelectionItemsProperty, value);
    }
    public static readonly DependencyProperty UsesSelectionItemsProperty
        = DependencyProperty.Register(
            nameof(UsesSelectionItems),
            typeof(bool),
            typeof(StswSegment)
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
            typeof(StswSegment),
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
            typeof(StswSegment),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion

    #region Animations
    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="isSelected"></param>
    private void AnimateSelectionChange(Border target, bool isSelected)
    {
        Color fromBackgroundColor = (isSelected
            ? (SolidColorBrush)FindResource("StswItem.Static.Background")
            : (SolidColorBrush)FindResource("StswItem.Checked.Static.Background")).Color;

        Color toBackgroundColor = (isSelected
            ? (SolidColorBrush)FindResource("StswItem.Checked.Static.Background")
            : (SolidColorBrush)FindResource("StswItem.Static.Background")).Color;

        Color fromBorderBrushColor = (isSelected
            ? (SolidColorBrush)FindResource("StswItem.Static.Border")
            : (SolidColorBrush)FindResource("StswItem.Checked.Static.Border")).Color;

        Color toBorderBrushColor = (isSelected
            ? (SolidColorBrush)FindResource("StswItem.Checked.Static.Border")
            : (SolidColorBrush)FindResource("StswItem.Static.Border")).Color;

        if (target.Background is not SolidColorBrush backgroundBrush || backgroundBrush.IsFrozen || !backgroundBrush.CanFreeze)
        {
            backgroundBrush = new SolidColorBrush(fromBackgroundColor);
            target.Background = backgroundBrush;
        }

        if (target.BorderBrush is not SolidColorBrush borderBrush || borderBrush.IsFrozen || !borderBrush.CanFreeze)
        {
            borderBrush = new SolidColorBrush(fromBorderBrushColor);
            target.BorderBrush = borderBrush;
        }

        var backgroundAnimation = new ColorAnimation
        {
            From = fromBackgroundColor,
            To = toBackgroundColor,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        var borderBrushAnimation = new ColorAnimation
        {
            From = fromBorderBrushColor,
            To = toBorderBrushColor,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        var storyboard = new Storyboard();

        storyboard.Children.Add(backgroundAnimation);
        Storyboard.SetTarget(backgroundAnimation, target);
        Storyboard.SetTargetProperty(backgroundAnimation, new PropertyPath("(Border.Background).(SolidColorBrush.Color)"));

        storyboard.Children.Add(borderBrushAnimation);
        Storyboard.SetTarget(borderBrushAnimation, target);
        Storyboard.SetTargetProperty(borderBrushAnimation, new PropertyPath("(Border.BorderBrush).(SolidColorBrush.Color)"));

        storyboard.Completed += (s, e) =>
        {
            target.ClearValue(Border.BackgroundProperty);
            target.ClearValue(Border.BorderBrushProperty);
        };

        storyboard.Begin();
    }
    #endregion
}
