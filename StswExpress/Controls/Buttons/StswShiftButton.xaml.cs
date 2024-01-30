using System;
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
public class StswShiftButton : ComboBox, IStswCornerControl
{
    public StswShiftButton()
    {
        DependencyPropertyDescriptor.FromProperty(IsReadOnlyProperty, typeof(ComboBox)).AddValueChanged(this, CheckButtonAccessibility);
        DependencyPropertyDescriptor.FromProperty(SelectedIndexProperty, typeof(ComboBox)).AddValueChanged(this, CheckButtonAccessibility);
    }
    static StswShiftButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswShiftButton), new FrameworkPropertyMetadata(typeof(StswShiftButton)));
    }

    #region Events & methods
    private ButtonBase? buttonPrevious, buttonNext;
    
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
            buttonPrevious.Click += (s, e) => ShiftBy(-1);
            this.buttonPrevious = buttonPrevious;
        }
        /// Button: next
        if (GetTemplateChild("PART_ButtonNext") is ButtonBase buttonNext)
        {
            buttonNext.IsEnabled = CanShiftBy(1) && !IsReadOnly;
            buttonNext.Click += (s, e) => ShiftBy(1);
            this.buttonNext = buttonNext;
        }
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

        if (IsEditable)
            return;

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
            SelectedIndex = (SelectedIndex + step % Items.Count + Items.Count) % Items.Count;
        else if (step > 0)
            SelectedIndex = Items.Count - 1;
        else
            SelectedIndex = 0;
    }

    /// <summary>
    /// Checks the accessibility and updates the enabled state of the previous and next shift buttons based on current conditions.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void CheckButtonAccessibility(object? sender, EventArgs e)
    {
        if (buttonPrevious != null && buttonNext != null)
        {
            buttonPrevious.IsEnabled = (IsLoopingEnabled || CanShiftBy(-1)) && !IsReadOnly;
            buttonNext.IsEnabled = (IsLoopingEnabled || CanShiftBy(1)) && !IsReadOnly;
        }
    }
    #endregion

    #region Main properties
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
            typeof(StswShiftButton),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsLoopingEnabledChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsLoopingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswShiftButton stsw)
        {
            stsw.CheckButtonAccessibility(null, EventArgs.Empty);
        }
    }
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
            typeof(StswShiftButton)
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
            typeof(StswShiftButton)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between arrow button and content presenter.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswShiftButton)
        );
    #endregion
}
