using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswShiftButton : ComboBox, IStswCornerControl
{
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
            buttonPrevious.Click += (s, e) => { CheckIndexAvailability(-1, out var newIndex); SelectedIndex = newIndex; };
            this.buttonPrevious = buttonPrevious;
        }
        /// Button: next
        if (GetTemplateChild("PART_ButtonNext") is ButtonBase buttonNext)
        {
            buttonNext.Click += (s, e) => { CheckIndexAvailability(1, out var newIndex); SelectedIndex = newIndex; };
            this.buttonNext = buttonNext;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool CheckIndexAvailability(int step, out int newIndex)
    {
        if (SelectedIndex + step >= Items.Count || SelectedIndex + step < 0)
        {
            newIndex = (SelectedIndex + step % Items.Count + Items.Count) % Items.Count;
            return false;
        }

        newIndex = SelectedIndex + step;
        return true;
    }
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
                if (stsw.IsLoopingEnabled)
                    stsw.buttonPrevious.IsEnabled = stsw.buttonPrevious.IsEnabled = true;
                else
                {
                    stsw.buttonPrevious.IsEnabled = stsw.CheckIndexAvailability(-1, out var _);
                    stsw.buttonNext.IsEnabled = stsw.CheckIndexAvailability(1, out var _);
                }
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
