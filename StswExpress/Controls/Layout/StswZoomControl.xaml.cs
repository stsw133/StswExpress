using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A zoomable container that enables users to zoom and pan its child content.
/// Supports mouse wheel zooming and drag-based panning.
/// </summary>
public class StswZoomControl : Border
{
    private UIElement? _child;
    private Point _origin, _start;

    static StswZoomControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswZoomControl), new FrameworkPropertyMetadata(typeof(StswZoomControl)));
    }

    #region Events & methods
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
        if (_child == null)
            return;

        var group = new TransformGroup();
        group.Children.Add(new ScaleTransform());
        group.Children.Add(new TranslateTransform());
        _child.RenderTransform = group;
        _child.RenderTransformOrigin = new Point(0.0, 0.0);

        MouseLeftButtonDown -= Child_MouseLeftButtonDown;
        MouseLeftButtonUp -= Child_MouseLeftButtonUp;
        MouseMove -= Child_MouseMove;
        MouseWheel -= Child_MouseWheel;
        PreviewMouseRightButtonDown -= Child_PreviewMouseRightButtonDown;

        MouseLeftButtonDown += Child_MouseLeftButtonDown;
        MouseLeftButtonUp += Child_MouseLeftButtonUp;
        MouseMove += Child_MouseMove;
        MouseWheel += Child_MouseWheel;
        PreviewMouseRightButtonDown += Child_PreviewMouseRightButtonDown;
    }

    /// <summary>
    /// Handles the left mouse button down event to initiate panning.
    /// Captures the mouse for dragging operations.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_child == null)
            return;

        var tt = GetTranslateTransform(_child);
        _start = e.GetPosition(this);
        _origin = new Point(tt.X, tt.Y);

        Cursor = Cursors.Hand;
        _child.CaptureMouse();
    }

    /// <summary>
    /// Handles the left mouse button up event to stop panning.
    /// Releases the mouse capture.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_child == null)
            return;

        _child.ReleaseMouseCapture();
        Cursor = Cursors.Arrow;
    }

    /// <summary>
    /// Handles the mouse move event to apply panning when the left mouse button is held down.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void Child_MouseMove(object sender, MouseEventArgs e)
    {
        if (_child?.IsMouseCaptured != true)
            return;

        var tt = GetTranslateTransform(_child);
        var v = _start - e.GetPosition(this);
        tt.X = _origin.X - v.X;
        tt.Y = _origin.Y - v.Y;
    }

    /// <summary>
    /// Handles the mouse wheel event to apply zooming based on the scroll direction.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_child == null)
            return;

        var st = GetScaleTransform(_child);
        var tt = GetTranslateTransform(_child);

        var zoomFactor = (e.Delta > 0) ? ZoomStep : (1.0 / ZoomStep);
        var newScale = st.ScaleX * zoomFactor;

        if (MinScale.HasValue && newScale < MinScale.Value)
            return;
        if (MaxScale.HasValue && newScale > MaxScale.Value)
            return;

        var relative = e.GetPosition(_child);
        var absX = relative.X * st.ScaleX + tt.X;
        var absY = relative.Y * st.ScaleY + tt.Y;

        st.ScaleX = newScale;
        st.ScaleY = newScale;

        tt.X = absX - relative.X * newScale;
        tt.Y = absY - relative.Y * newScale;

        ZoomPercentage = st.ScaleX * 100.0;
    }

    /// <summary>
    /// Handles the right mouse button down event to reset the zoom and panning.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    void Child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) => Reset();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private ScaleTransform GetScaleTransform(UIElement element) => (ScaleTransform)((TransformGroup)element.RenderTransform).Children.First(x => x is ScaleTransform);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private TranslateTransform GetTranslateTransform(UIElement element) => (TranslateTransform)((TransformGroup)element.RenderTransform).Children.First(x => x is TranslateTransform);

    /// <summary>
    /// Resets the zoom level and panning position of the zoom control.
    /// </summary>
    private void Reset()
    {
        if (_child == null)
            return;

        var st = GetScaleTransform(_child);
        st.ScaleX = 1.0;
        st.ScaleY = 1.0;

        var tt = GetTranslateTransform(_child);
        tt.X = 0.0;
        tt.Y = 0.0;

        ZoomPercentage = 100.0;
    }
    #endregion

    #region Logic properties
    /// <inheritdoc/>
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
    /// Gets or sets the maximum zoom scale.
    /// This prevents the content from being scaled above the specified value.
    /// </summary>
    public double? MaxScale
    {
        get => (double?)GetValue(MaxScaleProperty);
        set => SetValue(MaxScaleProperty, value);
    }
    public static readonly DependencyProperty MaxScaleProperty
        = DependencyProperty.Register(
            nameof(MaxScale),
            typeof(double?),
            typeof(StswZoomControl)
        );

    /// <summary>
    /// Gets or sets the minimum zoom scale.
    /// This prevents the content from being scaled below the specified value.
    /// </summary>
    public double? MinScale
    {
        get => (double?)GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }
    public static readonly DependencyProperty MinScaleProperty
        = DependencyProperty.Register(
            nameof(MinScale),
            typeof(double?),
            typeof(StswZoomControl)
        );

    /// <summary>
    /// 
    /// </summary>
    public double ZoomStep
    {
        get => (double)GetValue(ZoomStepProperty);
        set => SetValue(ZoomStepProperty, value);
    }
    public static readonly DependencyProperty ZoomStepProperty
        = DependencyProperty.Register(
            nameof(ZoomStep),
            typeof(double),
            typeof(StswZoomControl)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// 
    /// </summary>
    public double ZoomPercentage
    {
        get => (double)GetValue(ZoomPercentageProperty);
        private set => SetValue(ZoomPercentagePropertyKey, value);
    }
    private static readonly DependencyPropertyKey ZoomPercentagePropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(ZoomPercentage),
            typeof(double),
            typeof(StswZoomControl),
            new FrameworkPropertyMetadata(100.0)
        );
    public static readonly DependencyProperty ZoomPercentageProperty = ZoomPercentagePropertyKey!.DependencyProperty;
    #endregion
}

/* usage:

<se:StswZoomControl MinScale="0.5" MaxScale="4.0">
    <Image Source="example.jpg" Stretch="None"/>
</se:StswZoomControl>

*/
