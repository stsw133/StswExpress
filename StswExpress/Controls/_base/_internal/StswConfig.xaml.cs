using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shell;

namespace StswExpress;

/// <summary>
/// Represents a configuration dialog box that behaves like a content dialog.
/// This control is used to manage application settings within a window or dialog context.
/// </summary>
internal class StswConfig : Control, IStswCornerControl
{
    internal StswConfig(object? identifier)
    {
        Identifier = identifier;
    }
    static StswConfig()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswConfig), new FrameworkPropertyMetadata(typeof(StswConfig)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: confirm
        if (GetTemplateChild("PART_ButtonConfirm") is ButtonBase btnConfirm)
            btnConfirm.Click += (_, _) => Close(true);

        /// Button: cancel
        if (GetTemplateChild("PART_ButtonCancel") is ButtonBase btnCancel)
            btnCancel.Click += (_, _) => Close(false);

        /// Slider: iSize
        if (GetTemplateChild("PART_iSize") is Slider iSize)
            iSize.MouseLeave += (_, _) => StswSettings.Default.iSize = iSize.Value;
    }

    /// <summary>
    /// Closes the configuration dialog and applies or discards changes based on user choice.
    /// </summary>
    /// <param name="result">A boolean indicating whether changes should be saved (<see langword="true"/>) or discarded (<see langword="false"/>).</param>
    private void Close(bool? result)
    {
        if (Identifier is StswWindow stswWindow && stswWindow.ConfigPresentationMode == StswPresentationMode.Window)
            Window.GetWindow(this).Close();
        else
            StswContentDialog.Close(Identifier);

        if (result == true)
        {
            StswSettings.Default.Save();
        }
        else
        {
            StswSettings.Default.Reload();
            StswSettings.Default.Language = StswSettings.Default.Language;
            StswSettings.Default.Theme = StswSettings.Default.Theme;
        }
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
    /// 
    /// </summary>
    public string Version { get; set; } = Assembly.GetCallingAssembly().GetName().Version is Version v ? $"{v.Major}.{v.Minor}.{v.Revision}" : string.Empty;
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
            typeof(StswConfig),
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
            typeof(StswConfig),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
