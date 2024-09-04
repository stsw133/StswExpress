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

        OnIsMoveableChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Sets the offset of the tooltip based on the current mouse position.
    /// </summary>
    private void SetOffset()
    {
        var currentPos = Window.GetWindow(this).PointToScreen(Mouse.GetPosition(Window.GetWindow(this)));
        HorizontalOffset = currentPos.X + BorderThickness.Left;
        VerticalOffset = currentPos.Y + BorderThickness.Top + SystemParameters.CursorHeight / 2;
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
            if (stsw.IsMoveable)
            {
                stsw.Placement = PlacementMode.AbsolutePoint;

                stsw._parent = StswFn.GetParentPopup(stsw)?.PlacementTarget;
                if (stsw._parent == null)
                    stsw._parent = Window.GetWindow(stsw);
                if (stsw._parent != null)
                {
                    stsw.Opened += (_, _) => stsw.SetOffset();
                    stsw._parent.MouseMove += (_, _) => stsw.SetOffset();
                }
            }
            else if ((bool?)e.OldValue == true)
            {
                if (stsw._parent != null)
                {
                    stsw.Opened -= (_, _) => stsw.SetOffset();
                    stsw._parent.MouseMove -= (_, _) => stsw.SetOffset();
                }

                stsw.Placement = PlacementMode.Mouse;
                stsw.HorizontalOffset = 0;
                stsw.VerticalOffset = 0;
            }
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
