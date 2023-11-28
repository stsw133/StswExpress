using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a custom border control that enable user to zoom and move content.
/// </summary>
public class StswZoomControl : Border
{
    static StswZoomControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswZoomControl), new FrameworkPropertyMetadata(typeof(StswZoomControl)));
    }

    #region Events & methods
    private UIElement? child = null;
    private Point origin, start;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    public void Initialize(UIElement element)
    {
        child = element;
        if (child != null)
        {
            var group = new TransformGroup();
            group.Children.Add(new ScaleTransform());
            group.Children.Add(new TranslateTransform());
            child.RenderTransform = group;
            child.RenderTransformOrigin = new Point(0.0, 0.0);
            MouseLeftButtonDown += child_MouseLeftButtonDown;
            MouseLeftButtonUp += child_MouseLeftButtonUp;
            MouseMove += child_MouseMove;
            MouseWheel += child_MouseWheel;
            PreviewMouseRightButtonDown += new MouseButtonEventHandler(child_PreviewMouseRightButtonDown);
        }
    }

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
    /// 
    /// </summary>
    public void Reset()
    {
        if (child != null)
        {
            /// reset zoom
            var st = GetScaleTransform(child);
            st.ScaleX = 1.0;
            st.ScaleY = 1.0;

            /// reset pan
            var tt = GetTranslateTransform(child);
            tt.X = 0.0;
            tt.Y = 0.0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void child_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (child != null)
        {
            var st = GetScaleTransform(child);
            var tt = GetTranslateTransform(child);

            if (e.Delta <= 0 && (st.ScaleX < 0.4 || st.ScaleY < 0.4))
                return;

            var relative = e.GetPosition(child);
            var absoluteX = relative.X * st.ScaleX + tt.X;
            var absoluteY = relative.Y * st.ScaleY + tt.Y;

            var zoom = e.Delta > 0 ? 0.2 : -0.2;
            st.ScaleX += zoom;
            st.ScaleY += zoom;

            tt.X = absoluteX - relative.X * st.ScaleX;
            tt.Y = absoluteY - relative.Y * st.ScaleY;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (child != null)
        {
            var tt = GetTranslateTransform(child);
            start = e.GetPosition(this);
            origin = new Point(tt.X, tt.Y);
            Cursor = Cursors.Hand;
            child.CaptureMouse();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (child != null)
        {
            child.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) => Reset();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void child_MouseMove(object sender, MouseEventArgs e)
    {
        if (child?.IsMouseCaptured == true)
        {
            var tt = GetTranslateTransform(child);
            var v = start - e.GetPosition(this);
            tt.X = origin.X - v.X;
            tt.Y = origin.Y - v.Y;
        }
    }
    #endregion

    #region Main properties
    /// <summary>
    /// 
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
    #endregion
}
