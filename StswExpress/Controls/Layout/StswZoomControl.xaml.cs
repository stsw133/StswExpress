using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// A zoomable container that enables users to zoom and pan its child content.
/// Supports mouse wheel zooming and drag-based panning.
/// </summary>
public class StswZoomControl : Border
{
    static StswZoomControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswZoomControl), new FrameworkPropertyMetadata(typeof(StswZoomControl)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswZoomControl), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private UIElement? _child;
    private Point _origin, _start;

    /// <summary>
    /// Initializes the zoom control with the specified UI element,
    /// setting up necessary transformations for scaling and translation.
    /// </summary>
    /// <param name="element">The UI element to be zoomed and panned.</param>
    public void Initialize(UIElement element)
    {
        if (_child == element)
            return;

        _child = element;
        if (_child != null)
        {
            var group = new TransformGroup();
            group.Children.Add(new ScaleTransform());
            group.Children.Add(new TranslateTransform());
            _child.RenderTransform = group;
            _child.RenderTransformOrigin = new Point(0.0, 0.0);

            MouseLeftButtonDown -= child_MouseLeftButtonDown;
            MouseLeftButtonUp -= child_MouseLeftButtonUp;
            MouseMove -= child_MouseMove;
            MouseWheel -= child_MouseWheel;
            PreviewMouseRightButtonDown -= child_PreviewMouseRightButtonDown;

            MouseLeftButtonDown += child_MouseLeftButtonDown;
            MouseLeftButtonUp += child_MouseLeftButtonUp;
            MouseMove += child_MouseMove;
            MouseWheel += child_MouseWheel;
            PreviewMouseRightButtonDown += child_PreviewMouseRightButtonDown;
        }
    }

    /// <summary>
    /// Retrieves the scale transformation of the specified UI element.
    /// </summary>
    /// <param name="element">The UI element whose scale transform is retrieved.</param>
    /// <returns>The scale transformation applied to the element.</returns>
    private ScaleTransform GetScaleTransform(UIElement element) => (ScaleTransform)((TransformGroup)element.RenderTransform).Children.First(x => x is ScaleTransform);

    /// <summary>
    /// Retrieves the translation transformation of the specified UI element.
    /// </summary>
    /// <param name="element">The UI element whose translation transform is retrieved.</param>
    /// <returns>The translation transformation applied to the element.</returns>
    private TranslateTransform GetTranslateTransform(UIElement element) => (TranslateTransform)((TransformGroup)element.RenderTransform).Children.First(x => x is TranslateTransform);

    /// <summary>
    /// Resets the zoom level and panning position of the zoom control.
    /// </summary>
    public void Reset()
    {
        if (_child != null)
        {
            /// reset zoom
            var st = GetScaleTransform(_child);
            if (st.ScaleX != 1.0 || st.ScaleY != 1.0)
            {
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;
            }

            /// reset pan
            var tt = GetTranslateTransform(_child);
            if (tt.X != 0.0 || tt.Y != 0.0)
            {
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }
    }

    /// <summary>
    /// Handles the mouse wheel event to apply zooming based on the scroll direction.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void child_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_child != null)
        {
            var st = GetScaleTransform(_child);
            var tt = GetTranslateTransform(_child);

            var zoom = e.Delta > 0 ? 0.2 : -0.2;
            if (e.Delta <= 0 && (st.ScaleX + zoom < MinScale || st.ScaleY + zoom < MinScale))
                return;

            var relative = e.GetPosition(_child);
            var absoluteX = relative.X * st.ScaleX + tt.X;
            var absoluteY = relative.Y * st.ScaleY + tt.Y;

            st.ScaleX += zoom;
            st.ScaleY += zoom;

            tt.X = absoluteX - relative.X * st.ScaleX;
            tt.Y = absoluteY - relative.Y * st.ScaleY;
        }
    }

    /// <summary>
    /// Handles the left mouse button down event to initiate panning.
    /// Captures the mouse for dragging operations.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_child != null)
        {
            var tt = GetTranslateTransform(_child);
            _start = e.GetPosition(this);
            _origin = new Point(tt.X, tt.Y);
            Cursor = Cursors.Hand;
            _child.CaptureMouse();
        }
    }

    /// <summary>
    /// Handles the left mouse button up event to stop panning.
    /// Releases the mouse capture.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_child != null)
        {
            _child.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }
    }

    /// <summary>
    /// Handles the right mouse button down event to reset the zoom and panning.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) => Reset();

    /// <summary>
    /// Handles the mouse move event to apply panning when the left mouse button is held down.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void child_MouseMove(object sender, MouseEventArgs e)
    {
        if (_child?.IsMouseCaptured == true)
        {
            var tt = GetTranslateTransform(_child);
            var v = _start - e.GetPosition(this);
            tt.X = _origin.X - v.X;
            tt.Y = _origin.Y - v.Y;
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the child element of the zoom control.
    /// When a new child is assigned, it automatically initializes zoom and pan support.
    /// </summary>
    public override UIElement Child
    {
        get => base.Child;
        set
        {
            if (value != null && value != Child)
                Initialize(value);
            base.Child = value;
        }
    }

    /// <summary>
    /// Gets or sets the minimum zoom scale.
    /// This prevents the content from being scaled below the specified value.
    /// </summary>
    public double MinScale
    {
        get => (double)GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }
    public static readonly DependencyProperty MinScaleProperty
        = DependencyProperty.Register(
            nameof(MinScale),
            typeof(double),
            typeof(StswZoomControl),
            new FrameworkPropertyMetadata(0.4)
        );
    #endregion
}

/* usage:

<se:StswZoomControl MinScale="0.5">
    <Image Source="example.jpg"/>
</se:StswZoomControl>

*/
