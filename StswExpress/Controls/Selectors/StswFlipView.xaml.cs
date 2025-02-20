﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control that allows shifting through items using arrow buttons or keyboard input.
/// </summary>
public class StswFlipView : Selector, IStswCornerControl, IStswSelectionControl
{
    public StswFlipView()
    {
        DependencyPropertyDescriptor.FromProperty(IsReadOnlyProperty, typeof(Selector)).AddValueChanged(this, CheckButtonAccessibility);
        DependencyPropertyDescriptor.FromProperty(SelectedIndexProperty, typeof(Selector)).AddValueChanged(this, CheckButtonAccessibility);
    }
    static StswFlipView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFlipView), new FrameworkPropertyMetadata(typeof(StswFlipView)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswFlipView), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private ButtonBase? _buttonPrevious, _buttonNext;
    
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: previous
        if (GetTemplateChild("PART_ButtonPrevious") is ButtonBase buttonPrevious)
        {
            buttonPrevious.IsEnabled = CanShiftBy(-1) && !IsReadOnly;
            buttonPrevious.Click += (_, _) => ShiftBy(-1);
            _buttonPrevious = buttonPrevious;
        }
        /// Button: next
        if (GetTemplateChild("PART_ButtonNext") is ButtonBase buttonNext)
        {
            buttonNext.IsEnabled = CanShiftBy(1) && !IsReadOnly;
            buttonNext.Click += (_, _) => ShiftBy(1);
            _buttonNext = buttonNext;
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

    /// <summary>
    /// Overrides the behavior for mouse wheel input to shift through items if the control has focus and is not read-only.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);

        if (IsKeyboardFocusWithin && !IsReadOnly)
            ShiftBy(e.Delta > 0 ? -1 : 1);

        e.Handled = true;
    }

    /// <summary>
    /// Overrides the behavior for keyboard input to shift through items if the control has focus and is not read-only.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (IsKeyboardFocusWithin && !IsReadOnly)
        {
            switch (e.Key)
            {
                case Key.Left:
                case Key.Up:
                    ShiftBy(-1);
                    break;
                case Key.Down:
                case Key.Right:
                    ShiftBy(1);
                    break;
                case Key.PageDown:
                    ShiftBy(-10);
                    break;
                case Key.PageUp:
                    ShiftBy(10);
                    break;
                case Key.Home:
                    if (Items.Count > 0)
                        SelectedIndex = 0;
                    break;
                case Key.End:
                    if (Items.Count > 0)
                        SelectedIndex = Items.Count - 1;
                    break;
                case Key.Tab:
                    (Keyboard.FocusedElement as UIElement)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    break;
            }
        }
        e.Handled = true;
    }

    /// <summary>
    /// Determines whether shifting by a specified step is possible based on the current selected index and item count.
    /// </summary>
    /// <param name="step">The step value for shifting through items</param>
    /// <returns><see langword="true"/> if shifting by the given step is possible; <see langword="false"/> otherwise</returns>
    private bool CanShiftBy(int step)
    {
        if (SelectedIndex + step >= Items.Count || SelectedIndex + step < 0)
            return false;
        return true;
    }

    /// <summary>
    /// Shifts the selected index by the specified step, considering looping and boundary conditions.
    /// </summary>
    /// <param name="step">The step value for shifting through items</param>
    private void ShiftBy(int step)
    {
        if (Items.Count == 0)
            SelectedIndex = -1;
        else if (IsLoopingEnabled || CanShiftBy(step))
            SelectedIndex = (SelectedIndex + step + Items.Count) % Items.Count;
        else
            SelectedIndex = step > 0 ? Items.Count - 1 : 0;
    }

    /// <summary>
    /// Checks the accessibility and updates the enabled state of the previous and next shift buttons based on current conditions.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void CheckButtonAccessibility(object? sender, EventArgs e)
    {
        if (_buttonPrevious != null && _buttonNext != null)
        {
            _buttonPrevious.IsEnabled = (IsLoopingEnabled || CanShiftBy(-1)) && !IsReadOnly;
            _buttonNext.IsEnabled = (IsLoopingEnabled || CanShiftBy(1)) && !IsReadOnly;
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether looping through items is enabled when reaching the beginning or end.
    /// </summary>
    public bool IsLoopingEnabled
    {
        get => (bool)GetValue(IsLoopingEnabledProperty);
        set => SetValue(IsLoopingEnabledProperty, value);
    }
    public static readonly DependencyProperty IsLoopingEnabledProperty
        = DependencyProperty.Register(
            nameof(IsLoopingEnabled),
            typeof(bool),
            typeof(StswFlipView),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsLoopingEnabledChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsLoopingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFlipView stsw)
        {
            stsw.CheckButtonAccessibility(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 
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
            typeof(StswFlipView)
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
            typeof(StswFlipView),
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
            typeof(StswFlipView),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
