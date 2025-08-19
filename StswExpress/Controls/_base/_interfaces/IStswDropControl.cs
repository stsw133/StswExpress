using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress;

/// <summary>
/// Defines a contract for drop-down controls.
/// </summary>
[StswInfo("0.4.0")]
public interface IStswDropControl
{
    /// <summary>
    /// Gets or sets the path to a value on the source object to serve as the visual representation of the object.
    /// </summary>
    [StswInfo("0.19.0")]
    internal bool SuppressNextOpen { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether or not the drop-down portion of the control is currently open.
    /// </summary>
    public bool IsDropDownOpen { get; set; }
    public static readonly DependencyProperty? IsDropDownOpenProperty;

    /// <summary>
    /// Gets or sets the maximum height of the drop-down portion of the control.
    /// </summary>
    public double MaxDropDownHeight { get; set; }
    public static readonly DependencyProperty? MaxDropDownHeightProperty;

    /// <summary>
    /// Gets or sets the maximum width of the drop-down portion of the control.
    /// </summary>
    public double MaxDropDownWidth { get; set; }
    public static readonly DependencyProperty? MaxDropDownWidthProperty;

    /// <summary>
    /// Suppresses the next open action of the drop-down control to prevent it from opening immediately after being closed.
    /// </summary>
    /// <param name="obj">The drop-down control to suppress the next open action for.</param>
    /// <param name="e"> The event arguments containing the new value for the <see cref="IsDropDownOpen"/> property.</param>
    [StswInfo("0.19.0")]
    public static void IsDropDownOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not IStswDropControl dropControl)
            return;

        if ((bool)e.NewValue)
        {
            if (dropControl.SuppressNextOpen)
            {
                dropControl.SuppressNextOpen = false;
                obj.Dispatcher.BeginInvoke(() => dropControl.IsDropDownOpen = false, DispatcherPriority.Input);
                return;
            }

            Mouse.Capture(obj as IInputElement, CaptureMode.SubTree);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(obj, PreviewMouseDownOutsideCapturedElement);
        }
        else
        {
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(obj, PreviewMouseDownOutsideCapturedElement);
            if (Mouse.Captured == obj)
                Mouse.Capture(null);

            dropControl.SuppressNextOpen = true;
            obj.Dispatcher.BeginInvoke(() => dropControl.SuppressNextOpen = false, DispatcherPriority.Input);
        }
    }

    /// <summary>
    /// Handles the mouse down event outside the captured element to close the drop-down.
    /// </summary>
    /// <param name="sender">The drop-down control to handle the event for.</param>
    /// <param name="e">The mouse button event arguments.</param>
    [StswInfo("0.19.0")]
    public static void PreviewMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
    {
        if (sender is DependencyObject obj && obj is IStswDropControl)
        {
            var dp = StswFnUI.FindDependencyProperty(obj, nameof(IsDropDownOpen));
            if (dp != null)
                obj.SetCurrentValue(dp, false);
        }
    }
}
