using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control that extends the <see cref="ScrollViewer"/> class with additional functionality.
/// </summary>
public class StswShifter : ScrollViewer
{
    static StswShifter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswShifter), new FrameworkPropertyMetadata(typeof(StswShifter)));
    }

    #region Events & methods
    private ButtonBase? btnDown, btnLeft, btnRight, btnUp;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: down
        if (GetTemplateChild("PART_ButtonDown") is ButtonBase btnDown)
        {
            btnDown.Click += (s, e) => ScrollInfo.MouseWheelDown();
            this.btnDown = btnDown;
        }
        /// Button: left
        if (GetTemplateChild("PART_ButtonLeft") is ButtonBase btnLeft)
        {
            btnLeft.Click += (s, e) => ScrollInfo.MouseWheelLeft();
            this.btnLeft = btnLeft;
        }
        /// Button: right
        if (GetTemplateChild("PART_ButtonRight") is ButtonBase btnRight)
        {
            btnRight.Click += (s, e) => ScrollInfo.MouseWheelRight();
            this.btnRight = btnRight;
        }
        /// Button: up
        if (GetTemplateChild("PART_ButtonUp") is ButtonBase btnUp)
        {
            btnUp.Click += (s, e) => ScrollInfo.MouseWheelUp();
            this.btnUp = btnUp;
        }

        OnHorizontalOffsetChanged();
        OnVerticalOffsetChanged();
    }

    /// <summary>
    /// Overrides the MouseWheel event to handle scrolling behavior.
    /// When the scroll reaches the top or bottom, it raises the MouseWheel event for the parent UIElement.
    /// </summary>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        /// horizontal scrolling
        if (ComputedHorizontalScrollBarVisibility == Visibility.Visible
        && (ComputedVerticalScrollBarVisibility != Visibility.Visible || Keyboard.Modifiers == ModifierKeys.Shift))
        {
            if (e.Delta < 0)
                ScrollInfo.MouseWheelRight();
            else
                ScrollInfo.MouseWheelLeft();

            e.Handled = true;
        }

        base.OnMouseWheel(e);

        /// scrolling scroll in another scroll
        //if (Parent is UIElement parentElement)
        //{
        //    if ((e.Delta > 0 && VerticalOffset == 0) || (e.Delta < 0 && VerticalOffset == ScrollableHeight))
        //    {
        //        e.Handled = true;
        //
        //        var routedArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        //        {
        //            RoutedEvent = MouseWheelEvent
        //        };
        //        parentElement.RaiseEvent(routedArgs);
        //    }
        //}
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnScrollChanged(ScrollChangedEventArgs e)
    {
        if (e.HorizontalChange != 0)
        {
            if (btnLeft != null)
                btnLeft.IsEnabled = e.HorizontalOffset > 0;
            if (btnRight != null)
                btnRight.IsEnabled = e.HorizontalOffset + e.ViewportWidth < e.ExtentWidth;
        }
        if (e.VerticalChange != 0)
        {
            if (btnUp != null)
                btnUp.IsEnabled = e.VerticalOffset > 0;
            if (btnDown != null)
                btnDown.IsEnabled = e.VerticalOffset + e.ViewportHeight < e.ExtentHeight;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnHorizontalOffsetChanged()
    {
        if (btnLeft == null || btnRight == null)
            return;

        if (HorizontalOffset == 0)
            btnLeft.IsEnabled = false;
        else if (HorizontalOffset >= ScrollableWidth)
            btnRight.IsEnabled = false;
        else
        {
            btnLeft.IsEnabled = true;
            btnRight.IsEnabled = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnVerticalOffsetChanged()
    {
        if (btnUp == null || btnDown == null)
            return;

        if (VerticalOffset == 0)
            btnUp.IsEnabled = false;
        else if (VerticalOffset >= ScrollableWidth)
            btnDown.IsEnabled = false;
        else
        {
            btnUp.IsEnabled = true;
            btnDown.IsEnabled = true;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the down button.
    /// </summary>
    public Thickness ButtonDownBorderThickness
    {
        get => (Thickness)GetValue(ButtonDownBorderThicknessProperty);
        set => SetValue(ButtonDownBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty ButtonDownBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ButtonDownBorderThickness),
            typeof(Thickness),
            typeof(StswShifter)
        );

    /// <summary>
    /// Gets or sets the thickness of the left button.
    /// </summary>
    public Thickness ButtonLeftBorderThickness
    {
        get => (Thickness)GetValue(ButtonLeftBorderThicknessProperty);
        set => SetValue(ButtonLeftBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty ButtonLeftBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ButtonLeftBorderThickness),
            typeof(Thickness),
            typeof(StswShifter)
        );
    
    /// <summary>
    /// Gets or sets the thickness of the right button.
    /// </summary>
    public Thickness ButtonRightBorderThickness
    {
        get => (Thickness)GetValue(ButtonRightBorderThicknessProperty);
        set => SetValue(ButtonRightBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty ButtonRightBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ButtonRightBorderThickness),
            typeof(Thickness),
            typeof(StswShifter)
        );
    
    /// <summary>
    /// Gets or sets the thickness of the up button.
    /// </summary>
    public Thickness ButtonUpBorderThickness
    {
        get => (Thickness)GetValue(ButtonUpBorderThicknessProperty);
        set => SetValue(ButtonUpBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty ButtonUpBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ButtonUpBorderThickness),
            typeof(Thickness),
            typeof(StswShifter)
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
            typeof(StswShifter)
        );
    #endregion
}
