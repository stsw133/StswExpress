using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// Represents a message dialog control that behaves like a content dialog.
/// Supports customizable title, message, details, and predefined button layouts.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswMessageDialog Title="Warning" Message="Are you sure?" Buttons="YesNoCancel" IsOpen="True"/&gt;
/// </code>
/// </example>
public class StswMessageDialog : ContentControl, IStswCornerControl
{
    private ButtonBase? _buttonCopyToClipboard;
    public ICommand CloseCommand { get; }

    public StswMessageDialog()
    {
        CloseCommand = new StswCommand<bool?>(Close);
    }
    static StswMessageDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMessageDialog), new FrameworkPropertyMetadata(typeof(StswMessageDialog)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: copy to clipboard
        if (GetTemplateChild("PART_ButtonCopyToClipboard") is ButtonBase btnCopyToClipboard)
        {
            btnCopyToClipboard.Click += PART_ButtonCopyToClipboard_Click;
            _buttonCopyToClipboard = btnCopyToClipboard;
        }
    }

    /// <summary>
    /// Handles the copy-to-clipboard button click event.
    /// Copies the dialog's message and details (if available) to the clipboard.
    /// </summary>
    /// <param name="sender">The button triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void PART_ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(Details == null ? Message : $"{Message}{Environment.NewLine}{Details}");
        if (_buttonCopyToClipboard?.Content is StswTimedSwitch stsw)
            stsw.IsChecked = true;
    }
    #endregion

    #region Show & close
    /// <summary>
    /// Closes the message dialog and sets the result.
    /// </summary>
    /// <param name="result">The result value as a string, which is parsed into a boolean if not null.</param>
    private void Close(bool? result) => StswContentDialog.Close(Identifier, result);

    /// <summary>
    /// Shows the message dialog asynchronously with an exception message and details.
    /// </summary>
    /// <param name="ex">The exception whose message and details are displayed in the dialog.</param>
    /// <param name="title">The title of the dialog (optional).</param>
    /// <param name="saveLog">Indicates whether the message should be logged.</param>
    /// <param name="identifier">An identifier used to determine where the dialog should be shown.</param>
    /// <returns>The result of the dialog.</returns>
    public static async Task<bool?> Show(Exception ex, string? title = null, bool saveLog = true, object? identifier = null)
        => await Show(ex.Message, title, ex.ToString(), StswDialogButtons.OK, StswDialogImage.Error, saveLog, identifier);

    /// <summary>
    /// Shows the message dialog asynchronously with customizable content and options.
    /// </summary>
    /// <param name="message">The primary message displayed in the dialog.</param>
    /// <param name="title">The title of the dialog (optional).</param>
    /// <param name="details">Additional details displayed in the dialog (optional).</param>
    /// <param name="buttons">The button layout of the dialog.</param>
    /// <param name="image">The icon displayed in the dialog.</param>
    /// <param name="saveLog">Indicates whether the message should be logged.</param>
    /// <param name="identifier">An identifier used to determine where the dialog should be shown.</param>
    /// <returns>The result of the dialog.</returns>
    public static async Task<bool?> Show(string message, string? title = null, string? details = null, StswDialogButtons buttons = StswDialogButtons.OK, StswDialogImage image = StswDialogImage.None, bool saveLog = false, object? identifier = null)
    {
        StswMessageDialog dialog = new()
        {
            Title = title,
            Message = message,
            Details = details,
            Buttons = buttons,
            Image = image,
            Identifier = identifier ?? StswApp.StswWindow
        };
        if (saveLog)
        {
            if (Enum.TryParse(image.ToString(), out StswInfoType infoType))
                StswLog.Write(infoType, details ?? message);
        }

        return (bool?)await StswContentDialog.Show(dialog, dialog.Identifier);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the buttons displayed in the dialog (e.g., OK, Yes/No, Cancel).
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
    /// Gets or sets the additional details displayed in the dialog.
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
    /// Gets or sets the identifier used to determine where the dialog should be displayed.
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
    /// Gets or sets the image/icon displayed in the dialog.
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
    /// Gets or sets a value indicating whether the message dialog is currently open.
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
    /// Gets or sets the primary message displayed in the dialog.
    /// </summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    public static readonly DependencyProperty MessageProperty
        = DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(StswMessageDialog)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the details section of the dialog is visible.
    /// </summary>
    public bool ShowDetails
    {
        get => (bool)GetValue(ShowDetailsProperty);
        set => SetValue(ShowDetailsProperty, value);
    }
    public static readonly DependencyProperty ShowDetailsProperty
        = DependencyProperty.Register(
            nameof(ShowDetails),
            typeof(bool),
            typeof(StswMessageDialog),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null, null, false, UpdateSourceTrigger.PropertyChanged)
        );

    /// <summary>
    /// Gets or sets the title of the dialog.
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
            typeof(StswMessageDialog),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswMessageDialog),
            new FrameworkPropertyMetadata(default(CornerRadius),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
