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
[Stsw("0.4.0")]
public class StswToolTip : ToolTip, IStswCornerControl
{
    private UIElement? _parent;

    static StswToolTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToolTip), new FrameworkPropertyMetadata(typeof(StswToolTip)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateMoveableState();
    }

    /// <summary>
    /// Handles the MouseMove event for the parent element, dynamically updating the tooltip's position
    /// to follow the cursor as it moves, maintaining the tooltip's relative offset.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    [Stsw("0.7.0")]
    private void OnParentMouseMove(object sender, MouseEventArgs e) => SetOffset();

    /// <summary>
    /// Handles the tooltip's Opened event, setting the initial offset based on the current mouse position.
    /// Ensures the tooltip appears in the correct position when it becomes visible.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    [Stsw("0.7.0")]
    private void OnTooltipOpened(object sender, RoutedEventArgs e) => SetOffset();

    /// <summary>
    /// Resets the tooltip's positioning behavior when movement is disabled.
    /// Restores default placement and offset values.
    /// </summary>
    [Stsw("0.7.0")]
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
    /// Sets the offset of the tooltip based on the current mouse position.
    /// Ensures the tooltip appears at the correct location when displayed.
    /// </summary>
    [Stsw("0.7.0")]
    private void SetOffset()
    {
        if (Window.GetWindow(this) is Window window)
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
    [Stsw("0.7.0")]
    private void UpdateMoveableState()
    {
        if (IsMoveable)
        {
            Placement = PlacementMode.AbsolutePoint;
            _parent = StswFnUI.GetParentPopup(this)?.PlacementTarget ?? Window.GetWindow(this);

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
    #endregion

    #region Logic properties
    /// <summary>
    /// Attached property that sets the tooltip text for a <see cref="FrameworkElement"/>.
    /// </summary>
    [Stsw("0.19.0")]
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.RegisterAttached(
            nameof(TextProperty)[..^8],
            typeof(string),
            typeof(StswToolTip),
            new PropertyMetadata(null, OnTextChanged)
        );
    public static string? GetText(DependencyObject obj) => (string?)obj.GetValue(TextProperty);
    public static void SetText(DependencyObject obj, string? value) => obj.SetValue(TextProperty, value);
    private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not FrameworkElement stsw)
            return;

        if (e.NewValue is string text)
            stsw.ToolTip = new StswToolTip { Content = text };
        else
            stsw.ClearValue(ToolTipProperty);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the tooltip is moveable.
    /// When enabled, the tooltip dynamically follows the cursor's movement.
    /// </summary>
    [Stsw("0.7.0")]
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
        if (obj is not StswToolTip stsw)
            return;

        stsw.UpdateMoveableState();
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
