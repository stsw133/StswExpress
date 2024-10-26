using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Represents a window bar control with integrated window management and dynamic context menu.
/// Provides options for minimizing, maximizing, restoring, and entering/exiting fullscreen mode.
/// </summary>
[ContentProperty(nameof(Components))]
public class StswWindowBar : Control
{
    public StswWindowBar()
    {
        //SetValue(ComponentsProperty, new ObservableCollection<UIElement>()); // this code breaks the binding with StswWindow
    }
    static StswWindowBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswWindowBar), new FrameworkPropertyMetadata(typeof(StswWindowBar)));
    }

    #region Events & methods
    private StswWindow? _window;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _window = StswFn.FindVisualAncestor<StswWindow>(this);

        ConfigureButtons();
        ConfigureMenu();
    }

    /// <summary>
    /// Assigns click event handlers to window control buttons: minimize, restore, and close.
    /// </summary>
    private void ConfigureButtons()
    {
        if (_window == null)
            return;

        /// Button: minimize
        if (GetTemplateChild("PART_ButtonMinimize") is Button btnMinimize)
            btnMinimize.Click += (s, e) => _window.WindowState = WindowState.Minimized;

        /// Button: restate
        if (GetTemplateChild("PART_ButtonRestate") is Button btnRestate)
            btnRestate.Click += (s, e) =>
            {
                if (_window.Fullscreen)
                    _window.Fullscreen = false;
                else
                    _window.WindowState = _window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            };

        /// Button: close
        if (GetTemplateChild("PART_ButtonClose") is Button btnClose)
            btnClose.Click += (s, e) => _window.Close();
    }

    /// <summary>
    /// Initializes handlers and visibility settings for context menu items based on window state.
    /// </summary>
    private void ConfigureMenu()
    {
        if (_window == null || ContextMenu == null)
            return;

        ContextMenu.DataContext = _window;

        foreach (FrameworkElement item in ContextMenu.Items)
            ConfigureMenuItemHandlers(item);

        ContextMenu.Loaded += (s, e) =>
        {
            foreach (FrameworkElement item in ContextMenu.Items)
                ConfigureMenuItemVisibility(item);
        };
    }

    /// <summary>
    /// Configures the event handlers for a context menu item based on its name.
    /// </summary>
    /// <param name="item">The context menu item.</param>
    private void ConfigureMenuItemHandlers(FrameworkElement item)
    {
        if (_window == null || string.IsNullOrEmpty(item.Name) || item is not MenuItem menuItem)
            return;

        switch (item.Name)
        {
            case "PART_iConfig":
                menuItem.Click += (_, _) => StswConfig.Show(_window);
                break;

            case "PART_MenuDefault":
                menuItem.Click += (_, _) => _window.Default();
                break;

            case "PART_MenuMinimize":
                menuItem.Click += (_, _) => _window.WindowState = WindowState.Minimized;
                break;

            case "PART_MenuMaximize":
                menuItem.Click += (_, _) => _window.WindowState = WindowState.Maximized;
                break;

            case "PART_MenuRestore":
                menuItem.Click += (_, _) => _window.WindowState = WindowState.Normal;
                break;

            case "PART_MenuFullscreen":
                menuItem.Click += (_, _) => _window.Fullscreen = true;
                break;

            case "PART_MenuFullscreenExit":
                menuItem.Click += (_, _) => _window.Fullscreen = false;
                break;

            case "PART_MenuClose":
                menuItem.Click += (_, _) => _window.Close();
                break;
        }
    }

    /// <summary>
    /// Configures the visibility of a context menu item based on its name.
    /// </summary>
    /// <param name="item">The context menu item.</param>
    private void ConfigureMenuItemVisibility(FrameworkElement item)
    {
        if (_window == null || string.IsNullOrEmpty(item.Name))
            return;

        item.Visibility = item.Name switch
        {
            "PART_iConfig" => _window.ConfigPresentationMode != null,
            "PART_MenuConfigSeparator" => _window.ConfigPresentationMode != null,
            "PART_MenuDefault" => _window.ResizeMode.In(ResizeMode.CanResize, ResizeMode.CanResizeWithGrip),
            "PART_MenuMinimize" => _window.ResizeMode != ResizeMode.NoResize,
            "PART_MenuMaximize" => _window.ResizeMode.In(ResizeMode.CanResize, ResizeMode.CanResizeWithGrip) && _window.WindowState != WindowState.Maximized && !_window.Fullscreen,
            "PART_MenuRestore" => _window.ResizeMode.In(ResizeMode.CanResize, ResizeMode.CanResizeWithGrip) && _window.WindowState == WindowState.Maximized && !_window.Fullscreen,
            "PART_MenuFullscreen" => _window.ResizeMode.In(ResizeMode.CanResize, ResizeMode.CanResizeWithGrip) && !_window.Fullscreen,
            "PART_MenuFullscreenExit" => _window.ResizeMode.In(ResizeMode.CanResize, ResizeMode.CanResizeWithGrip) && _window.Fullscreen,
            "PART_MenuCloseSeparator" => _window.ResizeMode != ResizeMode.NoResize,
            "PART_MenuClose" => true,
            _ => true
        } ? Visibility.Visible : Visibility.Collapsed;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the collection of elements used in the window's title bar.
    /// </summary>
    public IList Components
    {
        get => (IList)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(IList),
            typeof(StswWindowBar)
        );
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
            typeof(StswWindowBar),
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
            typeof(StswWindowBar),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
