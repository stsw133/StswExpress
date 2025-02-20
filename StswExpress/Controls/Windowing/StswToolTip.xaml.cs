using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// A tooltip control that displays informative or descriptive content when hovering over an element.
/// Supports corner radius customization, dynamic positioning, and an optional moveable mode.
/// </summary>
/// <remarks>
/// When <see cref="IsMoveable"/> is enabled, the tooltip follows the cursor dynamically.
/// </remarks>
public class StswToolTip : ToolTip, IStswCornerControl
{
    static StswToolTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToolTip), new FrameworkPropertyMetadata(typeof(StswToolTip)));
    }

    #region Events & methods
    private UIElement? _parent;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateMoveableState();
    }

    /// <summary>
    /// Sets the offset of the tooltip based on the current mouse position.
    /// Ensures the tooltip appears at the correct location when displayed.
    /// </summary>
    private void SetOffset()
    {
        var window = Window.GetWindow(this);
        if (window != null)
        {
            var currentPos = window.PointToScreen(Mouse.GetPosition(window));

            HorizontalOffset = currentPos.X + BorderThickness.Left;
            VerticalOffset = currentPos.Y + BorderThickness.Top + SystemParameters.CursorHeight / 2;
        }
    }

    /// <summary>
    /// Updates the state of the tooltip movement based on the <see cref="IsMoveable"/> property.
    /// Enables dynamic positioning when movement is allowed.
    /// </summary>
    private void UpdateMoveableState()
    {
        if (IsMoveable)
        {
            Placement = PlacementMode.AbsolutePoint;
            _parent = StswFn.GetParentPopup(this)?.PlacementTarget ?? Window.GetWindow(this);

            if (_parent != null)
            {
                Opened += OnTooltipOpened;
                _parent.MouseMove += OnParentMouseMove;
            }
        }
        else
        {
            ResetMoveableState();
        }
    }

    /// <summary>
    /// Resets the tooltip's positioning behavior when movement is disabled.
    /// Restores default placement and offset values.
    /// </summary>
    private void ResetMoveableState()
    {
        if (_parent != null)
        {
            Opened -= OnTooltipOpened;
            _parent.MouseMove -= OnParentMouseMove;
            _parent = null;
        }

        Placement = PlacementMode.Mouse;
        HorizontalOffset = 0;
        VerticalOffset = 0;
    }

    /// <summary>
    /// Handles the tooltip's Opened event, setting the initial offset based on the current mouse position.
    /// Ensures the tooltip appears in the correct position when it becomes visible.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnTooltipOpened(object sender, RoutedEventArgs e) => SetOffset();

    /// <summary>
    /// Handles the MouseMove event for the parent element, dynamically updating the tooltip's position
    /// to follow the cursor as it moves, maintaining the tooltip's relative offset.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnParentMouseMove(object sender, MouseEventArgs e) => SetOffset();

    /// <summary>
    /// Updates the tooltip content for a specified <see cref="FrameworkElement"/> when the tooltip dependency property changes.
    /// If the new value is a string, it creates a custom tooltip using <see cref="StswToolTip"/> and assigns it to the target element.
    /// </summary>
    /// <param name="obj">The dependency object where the tooltip is applied.</param>
    /// <param name="e">The event arguments.</param>
    public static void OnToolTipChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is FrameworkElement stsw && e.NewValue is string tooltipContent)
            ToolTipService.SetToolTip(stsw, new StswToolTip { Content = tooltipContent });
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the tooltip is moveable.
    /// When enabled, the tooltip dynamically follows the cursor's movement.
    /// </summary>
    public bool IsMoveable
    {
        get => (bool)GetValue(IsMoveableProperty);
        set => SetValue(IsMoveableProperty, value);
    }
    public static readonly DependencyProperty IsMoveableProperty
        = DependencyProperty.Register(
            nameof(IsMoveable),
            typeof(bool),
            typeof(StswToolTip),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsMoveableChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsMoveableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswToolTip stsw)
        {
            stsw.UpdateMoveableState();
        }
    }
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswToolTip),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswToolTip),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<Button Content="Advanced Tooltip">
    <Button.ToolTip>
        <se:StswToolTip Content="Custom Tooltip" CornerRadius="6"/>
    </Button.ToolTip>
</Button>

*/
