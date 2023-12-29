using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Represents a custom window control with additional functionality and customization options.
/// </summary>
[ContentProperty(nameof(Components))]
public class StswWindowBar : Control
{
    public StswWindowBar()
    {
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());
    }
    static StswWindowBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswWindowBar), new FrameworkPropertyMetadata(typeof(StswWindowBar)));
    }

    #region Events & methods
    private StswWindow? stswWindow;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// StswWindow
        if (StswFn.FindVisualAncestor<StswWindow>(this) is StswWindow stswWindow)
            this.stswWindow = stswWindow;

        /// Button: minimize
        if (GetTemplateChild("PART_ButtonMinimize") is Button btnMinimize)
            btnMinimize.Click += MinimizeClick;
        /// Button: restore
        if (GetTemplateChild("PART_ButtonRestore") is Button btnRestore)
            btnRestore.Click += RestoreClick;
        /// Button: close
        if (GetTemplateChild("PART_ButtonClose") is Button btnClose)
            btnClose.Click += CloseClick;
    }

    /// <summary>
    /// Event handler for the minimize button click to minimize the window.
    /// </summary>
    protected void MinimizeClick(object? sender, RoutedEventArgs e)
    {
        if (stswWindow != null)
            stswWindow.WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Event handler for the restore button click to toggle between normal and maximized window state.
    /// </summary>
    protected void RestoreClick(object? sender, RoutedEventArgs e)
    {
        if (stswWindow != null)
        {
            if (stswWindow.Fullscreen == true)
                stswWindow.Fullscreen = false;
            else
                stswWindow.WindowState = stswWindow.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }
    }

    /// <summary>
    /// Event handler for the close button click to close the window.
    /// </summary>
    protected void CloseClick(object? sender, RoutedEventArgs e)
    {
        if (stswWindow != null)
            stswWindow.Close();
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the collection of UI elements used in the custom window's title bar.
    /// </summary>
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswWindowBar)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match the
    /// border's rounded corners, preventing elements from protruding beyond the border.
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
            typeof(StswWindowBar)
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
            typeof(StswWindowBar)
        );
    #endregion
}
