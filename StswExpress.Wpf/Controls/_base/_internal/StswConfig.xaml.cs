using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shell;

namespace StswExpress.Wpf;

/// <summary>
/// Represents a configuration dialog box that behaves like a content dialog.
/// This control is used to manage application settings within a window or dialog context.
/// </summary>
/// <remarks>
/// The dialog can be displayed as a standalone window or embedded within a content dialog.
/// It includes options for saving and discarding settings, as well as handling UI preferences.
/// </remarks>
internal class StswConfig : Control, IStswCornerControl
{
    private ButtonBase? _btnCancel;
    private ButtonBase? _btnConfirm;
    private Slider? _iSizeSlider;

    internal StswConfig(object? identifier)
    {
        Identifier = identifier;
    }
    static StswConfig()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswConfig), new FrameworkPropertyMetadata(typeof(StswConfig)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_btnConfirm != null)
            _btnConfirm.Click -= BtnConfirm_Click;
        if (_btnCancel != null)
            _btnCancel.Click -= BtnCancel_Click;
        if (_iSizeSlider != null)
            _iSizeSlider.MouseLeave -= ISize_MouseLeave;

        /// Button: confirm
        _btnConfirm = GetTemplateChild("PART_ButtonConfirm") as ButtonBase;
        if (_btnConfirm != null)
            _btnConfirm.Click += BtnConfirm_Click;

        /// Button: cancel
        _btnCancel = GetTemplateChild("PART_ButtonCancel") as ButtonBase;
        if (_btnCancel != null)
            _btnCancel.Click += BtnCancel_Click;

        /// Slider: iSize
        _iSizeSlider = GetTemplateChild("PART_iSize") as Slider;
        if (_iSizeSlider != null)
            _iSizeSlider.MouseLeave += ISize_MouseLeave;
    }

    /// <summary>
    /// Handles the click event of the cancel button, closing the dialog without saving changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close(false);

    /// <summary>
    /// Handles the click event of the confirm button, closing the dialog and saving changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void BtnConfirm_Click(object sender, RoutedEventArgs e) => Close(true);

    /// <summary>
    /// Handles the mouse leave event of the iSize slider, updating the setting when the user stops interacting with it.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void ISize_MouseLeave(object sender, MouseEventArgs e)
    {
        if (_iSizeSlider != null)
            StswApp.Settings.UiScale = _iSizeSlider.Value;
    }

    /// <summary>
    /// Closes the configuration dialog and applies or discards changes based on user choice.
    /// </summary>
    /// <param name="result">A boolean indicating whether changes should be saved (<see langword="true"/>) or discarded (<see langword="false"/>).</param>
    private async void Close(bool? result)
    {
        if (Identifier is StswWindow stswWindow && stswWindow.ConfigPresentationMode == StswPresentationMode.Window)
            Window.GetWindow(this).Close();
        else
            StswContentDialog.Close(Identifier);

        if (result == true)
            await StswSettings.SaveAsync(StswApp.Settings, false);
        else
            StswApp.Settings = await StswSettings.LoadAsync(false);
    }

    /// <summary>
    /// Displays the configuration dialog associated with the given identifier. 
    /// Can be shown as a dialog within a window or a standalone window depending on configuration.
    /// </summary>
    /// <param name="identifier">The identifier to determine the context where the dialog should be displayed.</param>
    public static async void Show(object identifier)
    {
        if (identifier is StswWindow window && window.ConfigPresentationMode == StswPresentationMode.Window)
        {
            var newWindow = new StswWindow()
            {
                ConfigPresentationMode = null,
                Content = new StswConfig(window),
                Icon = StswIcons.Cog.ToImageSource(24, SystemColors.WindowTextBrush),
                Owner = window,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                Title = StswTranslator.GetTranslation(nameof(StswConfig)),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            WindowChrome.SetWindowChrome(newWindow, new());
            newWindow.ShowDialog();
        }
        else
        {
            if (!StswContentDialog.GetInstance(identifier).IsOpen)
                await StswContentDialog.Show(new StswConfig(identifier), identifier);
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Identifier used in conjunction with <see cref="Show(object)"/> to determine where a dialog should be shown.
    /// The identifier helps in deciding the window or dialog context for displaying the configuration UI.
    /// </summary>
    public object? Identifier
    {
        get => GetValue(IdentifierProperty);
        set => SetValue(IdentifierProperty, value);
    }
    public static readonly DependencyProperty IdentifierProperty
        = DependencyProperty.Register(
            nameof(Identifier),
            typeof(object),
            typeof(StswConfig)
        );

    /// <summary>
    /// Gets the application version from the calling assembly in "Major.Minor.Revision" format.
    /// </summary>
    public string Version { get; } = Assembly.GetCallingAssembly().GetName().Version is Version v ? $"{v.Major}.{v.Minor}.{v.Build}" : string.Empty;
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
            typeof(StswConfig)
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
            typeof(StswConfig)
        );
    #endregion
}
