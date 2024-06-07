using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control behaving like content dialog with various properties for customization.
/// </summary>
public class StswMessageDialog : Control, IStswCornerControl
{
    public StswMessageDialog()
    {
        CloseCommand = new StswCommand<string?>(Close);
    }
    static StswMessageDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMessageDialog), new FrameworkPropertyMetadata(typeof(StswMessageDialog)));
    }

    #region Events & methods
    private ButtonBase? _buttonCopyToClipboard;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: copy to clipboard
        if (GetTemplateChild("PART_ButtonCopyToClipboard") is ButtonBase buttonCopyToClipboard)
        {
            buttonCopyToClipboard.Click += PART_ButtonCopyToClipboard_Click;
            _buttonCopyToClipboard = buttonCopyToClipboard;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(Details == null ? Content : $"{Content} {Details}");
        if (_buttonCopyToClipboard?.Content is StswTimedSwitch stsw)
            stsw.IsChecked = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public ICommand CloseCommand { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    private void Close(string? result) => StswContentDialog.Close(Identifier, result != null ? bool.Parse(result) : null);
    #endregion

    #region Show methods
    /// <summary>
    /// Shows the message dialog asynchronously.
    /// </summary>
    /// <param name="ex">The exception to display content and details of the dialog.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="saveLog">Set if details or content should be saved to log file.</param>
    /// <param name="identifier">An identifier used to determine where a dialog should be shown.</param>
    /// <returns>The result of the dialog.</returns>
    public static async Task<bool?> Show(Exception ex, string? title = null, bool saveLog = false, object? identifier = null)
        => await Show(ex.Message, title, ex.ToString(), StswDialogButtons.OK, StswDialogImage.Error, saveLog, identifier);

    /// <summary>
    /// Shows the message dialog asynchronously.
    /// </summary>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="buttons">The buttons to be displayed in the dialog.</param>
    /// <param name="image">The image to be displayed in the dialog.</param>
    /// <param name="saveLog">Set if details or content should be saved to log file.</param>
    /// <param name="identifier">An identifier used to determine where a dialog should be shown.</param>
    /// <returns>The result of the dialog.</returns>
    public static async Task<bool?> Show(string content, string? title = null, StswDialogButtons buttons = StswDialogButtons.OK, StswDialogImage image = StswDialogImage.None, bool saveLog = false, object? identifier = null)
        => await Show(content, title, null, buttons, image, saveLog, identifier);

    /// <summary>
    /// Shows the message dialog asynchronously.
    /// </summary>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="details">The details of the dialog.</param>
    /// <param name="buttons">The buttons to be displayed in the dialog.</param>
    /// <param name="image">The image to be displayed in the dialog.</param>
    /// <param name="saveLog">Set if details or content should be saved to log file.</param>
    /// <param name="identifier">An identifier used to determine where a dialog should be shown.</param>
    /// <returns>The result of the dialog.</returns>
    public static async Task<bool?> Show(string content, string? title = null, string? details = null, StswDialogButtons buttons = StswDialogButtons.OK, StswDialogImage image = StswDialogImage.None, bool saveLog = false, object? identifier = null)
    {
        StswMessageDialog dialog = new()
        {
            Title = title,
            Content = content,
            Details = details,
            Buttons = buttons,
            Image = image,
            Identifier = identifier ?? StswApp.StswWindow
        };
        if (saveLog)
        {
            Enum.TryParse(image.ToString(), out StswInfoType infoType);
            StswLog.Write(infoType, details ?? content);
        }

        return (bool?)await StswContentDialog.Show(dialog, dialog.Identifier);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// 
    /// </summary>
    public StswDialogButtons Buttons
    {
        get => (StswDialogButtons)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(StswDialogButtons),
            typeof(StswMessageDialog)
        );

    /// <summary>
    /// 
    /// </summary>
    public string Content
    {
        get => (string)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    public static readonly DependencyProperty ContentProperty
        = DependencyProperty.Register(
            nameof(Content),
            typeof(string),
            typeof(StswMessageDialog)
        );

    /// <summary>
    /// 
    /// </summary>
    public string? Details
    {
        get => (string?)GetValue(DetailsProperty);
        set => SetValue(DetailsProperty, value);
    }
    public static readonly DependencyProperty DetailsProperty
        = DependencyProperty.Register(
            nameof(Details),
            typeof(string),
            typeof(StswMessageDialog)
        );

    /// <summary>
    /// Identifier which is used in conjunction with <see cref="Show(object)"/> to determine where a dialog should be shown.
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
            typeof(StswMessageDialog)
        );

    /// <summary>
    /// 
    /// </summary>
    public StswDialogImage Image
    {
        get => (StswDialogImage)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }
    public static readonly DependencyProperty ImageProperty
        = DependencyProperty.Register(
            nameof(Image),
            typeof(StswDialogImage),
            typeof(StswMessageDialog)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the content dialog is open or not.
    /// </summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    public static readonly DependencyProperty IsOpenProperty
        = DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(StswMessageDialog),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null, null, false, UpdateSourceTrigger.PropertyChanged)
        );

    /// <summary>
    /// 
    /// </summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty
        = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(StswMessageDialog)
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
            typeof(StswMessageDialog)
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
            typeof(StswMessageDialog)
        );
    #endregion
}
