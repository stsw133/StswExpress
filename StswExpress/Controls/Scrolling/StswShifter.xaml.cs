using System.Windows;
using System.Windows.Controls;
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
    private StswRepeatButton? partButtonUp, partButtonDown, partButtonLeft, partButtonRight;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: up
        if (GetTemplateChild("PART_ButtonUp") is StswRepeatButton partButtonUp)
        {
            partButtonUp.Click += BtnUp_Click;
            this.partButtonUp = partButtonUp;
        }
        /// Button: down
        if (GetTemplateChild("PART_ButtonDown") is StswRepeatButton partButtonDown)
        {
            partButtonDown.Click += BtnDown_Click;
            this.partButtonDown = partButtonDown;
        }
        /// Button: left
        if (GetTemplateChild("PART_ButtonLeft") is StswRepeatButton partButtonLeft)
        {
            partButtonLeft.Click += BtnLeft_Click;
            this.partButtonLeft = partButtonLeft;
        }
        /// Button: right
        if (GetTemplateChild("PART_ButtonRight") is StswRepeatButton partButtonRight)
        {
            partButtonRight.Click += BtnRight_Click;
            this.partButtonRight = partButtonRight;
        }

        OnHorizontalOffsetChanged();
        OnVerticalOffsetChanged();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnDown_Click(object sender, RoutedEventArgs e) => ScrollInfo.MouseWheelDown();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnLeft_Click(object sender, RoutedEventArgs e) => ScrollInfo.MouseWheelLeft();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnRight_Click(object sender, RoutedEventArgs e) => ScrollInfo.MouseWheelRight();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnUp_Click(object sender, RoutedEventArgs e) => ScrollInfo.MouseWheelUp();

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
            if (partButtonLeft != null)
                partButtonLeft.IsEnabled = e.HorizontalOffset > 0;
            if (partButtonRight != null)
                partButtonRight.IsEnabled = e.HorizontalOffset + e.ViewportWidth < e.ExtentWidth;
        }
        if (e.VerticalChange != 0)
        {
            if (partButtonUp != null)
                partButtonUp.IsEnabled = e.VerticalOffset > 0;
            if (partButtonDown != null)
                partButtonDown.IsEnabled = e.VerticalOffset + e.ViewportHeight < e.ExtentHeight;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnHorizontalOffsetChanged()
    {
        if (partButtonLeft == null || partButtonRight == null)
            return;

        if (HorizontalOffset == 0)
            partButtonLeft.IsEnabled = false;
        else if (HorizontalOffset >= ScrollableWidth)
            partButtonRight.IsEnabled = false;
        else
        {
            partButtonLeft.IsEnabled = true;
            partButtonRight.IsEnabled = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnVerticalOffsetChanged()
    {
        if (partButtonUp == null || partButtonDown == null)
            return;

        if (VerticalOffset == 0)
            partButtonUp.IsEnabled = false;
        else if (VerticalOffset >= ScrollableWidth)
            partButtonDown.IsEnabled = false;
        else
        {
            partButtonUp.IsEnabled = true;
            partButtonDown.IsEnabled = true;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
