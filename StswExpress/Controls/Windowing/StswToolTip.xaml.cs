using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// Represents a control for displaying informative or descriptive content when hovering over an element.
/// </summary>
public class StswToolTip : ToolTip, IStswCornerControl
{
    static StswToolTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToolTip), new FrameworkPropertyMetadata(typeof(StswToolTip)));
    }

    #region Events & methods
    private UIElement? _parent;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateMoveableState();
    }

    /// <summary>
    /// Sets the offset of the tooltip based on the current mouse position.
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
    /// Updates the state of the tooltip movement based on the IsMoveable property.
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
    /// Resets the tooltip to default placement and offsets when movement is disabled.
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
    /// This ensures the tooltip appears in the correct position when it becomes visible.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnTooltipOpened(object sender, RoutedEventArgs e) => SetOffset();

    /// <summary>
    /// Handles the MouseMove event for the parent element, dynamically updating the tooltip's position
    /// to follow the cursor as it moves, maintaining the tooltip's relative offset.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnParentMouseMove(object sender, MouseEventArgs e) => SetOffset();

    /// <summary>
    /// Updates the tooltip content for a specified <see cref="FrameworkElement"/> when the tooltip dependency property changes.
    /// If the new value is a string, it creates a custom tooltip using <see cref="StswToolTip"/> and assigns it to the target element.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    public static void OnToolTipChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is FrameworkElement stsw && e.NewValue is string tooltipContent)
            ToolTipService.SetToolTip(stsw, new StswToolTip { Content = tooltipContent });
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the tooltip is moveable.
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
            typeof(StswToolTip),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswToolTip),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
