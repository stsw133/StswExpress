using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace StswExpress.Avalonia;
/// <summary>
/// A <see cref="ScrollViewer"/> extension with additional directional buttons for scrolling.
/// Supports optional dynamic visibility for the navigation buttons.
/// </summary>
/// <example>
/// <code>
/// &lt;se:StswDirectionView Orientation="Horizontal" DynamicMode="Partial"&gt;
///     &lt;StackPanel Orientation="Horizontal"&gt;
///         &lt;TextBlock Text="Item 1" /&gt;
///         &lt;TextBlock Text="Item 2" /&gt;
///     &lt;/StackPanel&gt;
/// &lt;/se:StswDirectionView&gt;
/// </code>
/// </example>
public class StswDirectionView : ScrollViewer
{
    private RepeatButton? _btnDown, _btnLeft, _btnRight, _btnUp;

    static StswDirectionView()
    {
        DynamicModeProperty.Changed.AddClassHandler<StswDirectionView>((s, _) => s.UpdateDynamicMode());
        OrientationProperty.Changed.AddClassHandler<StswDirectionView>((s, _) => s.UpdateScrollVisibility());
    }
    public StswDirectionView()
    {
        ScrollChanged += OnScrollChanged;
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _btnDown = e.NameScope.Find<RepeatButton>("PART_ButtonDown");
        _btnLeft = e.NameScope.Find<RepeatButton>("PART_ButtonLeft");
        _btnRight = e.NameScope.Find<RepeatButton>("PART_ButtonRight");
        _btnUp = e.NameScope.Find<RepeatButton>("PART_ButtonUp");

        AttachButton(_btnDown, () => LineDown());
        AttachButton(_btnUp, () => LineUp());
        AttachButton(_btnLeft, () => LineLeft());
        AttachButton(_btnRight, () => LineRight());

        UpdateDynamicMode();
        UpdateScrollVisibility();
        UpdateButtonsState(Offset);
    }

    /// <summary>
    /// Updates button enablement when the scroll position changes.
    /// </summary>
    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        UpdateButtonsState(new Vector(Offset.X, Offset.Y));
    }

    private void AttachButton(RepeatButton? button, Action action)
    {
        if (button == null)
            return;

        button.Click += (_, _) => action();
        button.AddHandler(PointerPressedEvent, (_, __) => button.Classes.Add("active"), RoutingStrategies.Tunnel);
        button.AddHandler(PointerReleasedEvent, (_, __) => button.Classes.Remove("active"), RoutingStrategies.Tunnel);
    }

    private void UpdateButtonsState(Vector offset)
    {
        if (_btnLeft != null)
            _btnLeft.IsEnabled = offset.X > 0;
        if (_btnRight != null && Extent.Width > Viewport.Width)
            _btnRight.IsEnabled = offset.X + Viewport.Width < Extent.Width;
        if (_btnUp != null)
            _btnUp.IsEnabled = offset.Y > 0;
        if (_btnDown != null && Extent.Height > Viewport.Height)
            _btnDown.IsEnabled = offset.Y + Viewport.Height < Extent.Height;
    }

    private void UpdateScrollVisibility()
    {
        if (Orientation == Orientation.Horizontal)
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }
        else
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }
    }

    private void UpdateDynamicMode()
    {
        PseudoClasses.Set(":dynamic-partial", DynamicMode == StswDynamicVisibilityMode.Partial);
        PseudoClasses.Set(":dynamic-full", DynamicMode == StswDynamicVisibilityMode.Full);
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the dynamic visibility mode for the navigation buttons.
    /// </summary>
    public StswDynamicVisibilityMode DynamicMode
    {
        get => GetValue(DynamicModeProperty);
        set => SetValue(DynamicModeProperty, value);
    }
    public static readonly StyledProperty<StswDynamicVisibilityMode> DynamicModeProperty = AvaloniaProperty.Register<StswDirectionView, StswDynamicVisibilityMode>(nameof(DynamicMode));

    /// <summary>
    /// Gets or sets the orientation of the control (horizontal or vertical).
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<StswDirectionView, Orientation>(nameof(Orientation), Orientation.Horizontal);
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the back (up and left) buttons.
    /// </summary>
    public Thickness BBtnThickness
    {
        get => GetValue(BBtnThicknessProperty);
        set => SetValue(BBtnThicknessProperty, value);
    }
    public static readonly StyledProperty<Thickness> BBtnThicknessProperty = AvaloniaProperty.Register<StswDirectionView, Thickness>(nameof(BBtnThickness), new Thickness(2));

    /// <summary>
    /// Gets or sets the thickness of the forward (down and right) buttons.
    /// </summary>
    public Thickness FBtnThickness
    {
        get => GetValue(FBtnThicknessProperty);
        set => SetValue(FBtnThicknessProperty, value);
    }
    public static readonly StyledProperty<Thickness> FBtnThicknessProperty = AvaloniaProperty.Register<StswDirectionView, Thickness>(nameof(FBtnThickness), new Thickness(2));
    #endregion
}
