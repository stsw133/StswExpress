using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswShiftButton : ComboBox, IStswCornerControl
{
    public StswShiftButton()
    {
        DependencyPropertyDescriptor.FromProperty(SelectedIndexProperty, typeof(ProgressBar)).AddValueChanged(this, OnSelectedIndexChanged);
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
            buttonPrevious.IsEnabled = CanShiftBy(-1);
            buttonPrevious.Click += (s, e) => ShiftBy(-1);
            this.buttonPrevious = buttonPrevious;
        }
        /// Button: next
        if (GetTemplateChild("PART_ButtonNext") is ButtonBase buttonNext)
        {
            buttonNext.IsEnabled = CanShiftBy(1);
            buttonNext.Click += (s, e) => ShiftBy(1);
            this.buttonNext = buttonNext;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);

        if (IsKeyboardFocusWithin)
        {
            if (e.Delta > 0)
                ShiftBy(1);
            else
                ShiftBy(-1);
        }
        e.Handled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    private bool CanShiftBy(int step)
    {
        if (SelectedIndex + step >= Items.Count || SelectedIndex + step < 0)
            return false;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
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
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSelectedIndexChanged(object? sender, EventArgs e) => OnIsLoopingEnabledChanged(this, new DependencyPropertyChangedEventArgs());
    #endregion

    #region Main properties
    /// <summary>
    /// 
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
            if (stsw.buttonPrevious != null && stsw.buttonNext != null)
            {
                stsw.buttonPrevious.IsEnabled = stsw.IsLoopingEnabled || stsw.CanShiftBy(-1);
                stsw.buttonNext.IsEnabled = stsw.IsLoopingEnabled || stsw.CanShiftBy(1);
            }
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match the
    /// border's rounded corners, preventing elements from protruding beyond the border.
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
    #endregion
}
