using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control that extends the <see cref="ScrollViewer"/> class with additional functionality.
/// </summary>
public class StswDirectionView : ScrollViewer, IStswCornerControl
{
    static StswDirectionView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDirectionView), new FrameworkPropertyMetadata(typeof(StswDirectionView)));
    }

    #region Events & methods
    private ButtonBase? _btnDown, _btnLeft, _btnRight, _btnUp;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: down
        if (GetTemplateChild("PART_ButtonDown") is ButtonBase btnDown)
        {
            btnDown.Click += (_, _) => ScrollInfo.MouseWheelDown();
            _btnDown = btnDown;
        }
        /// Button: left
        if (GetTemplateChild("PART_ButtonLeft") is ButtonBase btnLeft)
        {
            btnLeft.Click += (_, _) => ScrollInfo.MouseWheelLeft();
            _btnLeft = btnLeft;
        }
        /// Button: right
        if (GetTemplateChild("PART_ButtonRight") is ButtonBase btnRight)
        {
            btnRight.Click += (_, _) => ScrollInfo.MouseWheelRight();
            _btnRight = btnRight;
        }
        /// Button: up
        if (GetTemplateChild("PART_ButtonUp") is ButtonBase btnUp)
        {
            btnUp.Click += (_, _) => ScrollInfo.MouseWheelUp();
            _btnUp = btnUp;
        }

        OnHorizontalOffsetChanged();
        OnVerticalOffsetChanged();
    }

    /// <summary>
    /// Overrides the MouseWheel event to handle scrolling behavior.
    /// When the scroll reaches the top or bottom, it raises the MouseWheel event for the parent UIElement.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        /// horizontal scrolling
        if (ComputedHorizontalScrollBarVisibility == Visibility.Visible
        && (ComputedVerticalScrollBarVisibility != Visibility.Visible || Keyboard.Modifiers == ModifierKeys.Shift))
        {
            if (e.Delta > 0)
                ScrollInfo.MouseWheelLeft();
            else //if (e.Delta < 0)
                ScrollInfo.MouseWheelRight();

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
    /// Handles the ScrollChanged event to provide additional functionality on scroll change.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnScrollChanged(ScrollChangedEventArgs e)
    {
        if (e.HorizontalChange != 0)
        {
            if (_btnLeft != null)
                _btnLeft.IsEnabled = e.HorizontalOffset > 0;
            if (_btnRight != null)
                _btnRight.IsEnabled = e.HorizontalOffset + e.ViewportWidth < e.ExtentWidth;
        }
        if (e.VerticalChange != 0)
        {
            if (_btnUp != null)
                _btnUp.IsEnabled = e.VerticalOffset > 0;
            if (_btnDown != null)
                _btnDown.IsEnabled = e.VerticalOffset + e.ViewportHeight < e.ExtentHeight;
        }
    }

    /// <summary>
    /// Handles changes in horizontal offset.
    /// Updates the state of directional buttons based on horizontal offset.
    /// </summary>
    private void OnHorizontalOffsetChanged()
    {
        if (_btnLeft == null || _btnRight == null)
            return;

        if (HorizontalOffset == 0)
            _btnLeft.IsEnabled = false;
        else if (HorizontalOffset >= ScrollableWidth)
            _btnRight.IsEnabled = false;
        else
        {
            _btnLeft.IsEnabled = true;
            _btnRight.IsEnabled = true;
        }
    }

    /// <summary>
    /// Handles changes in vertical offset.
    /// Updates the state of directional buttons based on vertical offset.
    /// </summary>
    private void OnVerticalOffsetChanged()
    {
        if (_btnUp == null || _btnDown == null)
            return;

        if (VerticalOffset == 0)
            _btnUp.IsEnabled = false;
        else if (VerticalOffset >= ScrollableWidth)
            _btnDown.IsEnabled = false;
        else
        {
            _btnUp.IsEnabled = true;
            _btnDown.IsEnabled = true;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the back (up and left) buttons.
    /// </summary>
    public Thickness BBtnThickness
    {
        get => (Thickness)GetValue(BBtnThicknessProperty);
        set => SetValue(BBtnThicknessProperty, value);
    }
    public static readonly DependencyProperty BBtnThicknessProperty
        = DependencyProperty.Register(
            nameof(BBtnThickness),
            typeof(Thickness),
            typeof(StswDirectionView)
        );

    /// <summary>
    /// Gets or sets the thickness of the forward (down and right) buttons.
    /// </summary>
    public Thickness FBtnThickness
    {
        get => (Thickness)GetValue(FBtnThicknessProperty);
        set => SetValue(FBtnThicknessProperty, value);
    }
    public static readonly DependencyProperty FBtnThicknessProperty
        = DependencyProperty.Register(
            nameof(FBtnThickness),
            typeof(Thickness),
            typeof(StswDirectionView)
        );

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
            typeof(StswDirectionView)
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
            typeof(StswDirectionView)
        );
    #endregion
}
