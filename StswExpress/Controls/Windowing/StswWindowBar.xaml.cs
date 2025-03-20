using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A window bar control that provides integrated window management options.
/// Supports minimize, maximize, restore, fullscreen toggle, and context menu actions.
/// </summary>
/// <remarks>
/// Designed to be used inside a <see cref="StswWindow"/> to offer window control buttons and a context menu.
/// The bar automatically detects its parent window and configures its actions accordingly.
/// </remarks>
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

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _window = StswAppFn.FindVisualAncestor<StswWindow>(this);

        ConfigureButtons();
        ConfigureMenu();
    }

    /// <summary>
    /// Configures window control buttons, assigning event handlers to enable minimize, maximize, restore, and close actions.
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
    /// Initializes the context menu with appropriate event handlers and dynamically adjusts menu visibility based on the window's state.
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
    /// Ensures that each item performs the correct action when clicked.
    /// </summary>
    /// <param name="item">The context menu item to configure.</param>
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
    /// Adjusts the visibility of context menu items based on the current window state.
    /// Ensures that only relevant options are displayed at any given moment.
    /// </summary>
    /// <param name="item">The context menu item whose visibility is being updated.</param>
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
    /// Gets or sets the collection of UI elements displayed in the window bar.
    /// Allows customization by adding additional controls to the bar.
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
            typeof(StswWindowBar),
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
            typeof(StswWindowBar),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswWindow>
    <se:StswWindowBar/>
</se:StswWindow>

*/
