using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// Represents a message dialog control that behaves like a content dialog.
/// Supports customizable title, message, details, and predefined button layouts.
/// </summary>
internal class StswProgressDialog : ContentControl, IStswCornerControl
{
    public ICommand CloseCommand { get; }

    public StswProgressDialog()
    {
        CloseCommand = new StswCommand<string?>(Close);
    }
    static StswProgressDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswProgressDialog), new FrameworkPropertyMetadata(typeof(StswProgressDialog)));
    }

    #region Show & close
    /// <summary>
    /// Closes the message dialog and sets the result.
    /// </summary>
    /// <param name="result">The result value as a string, which is parsed into a boolean if not null.</param>
    private void Close(string? result) => StswContentDialog.Close(Identifier, result != null ? bool.Parse(result) : null);

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
    public static async Task<bool?> Show(string? title = null, object? identifier = null)
    {
        StswProgressDialog dialog = new()
        {
            Title = title,
            Identifier = identifier ?? StswApp.StswWindow
        };

        return (bool?)await StswContentDialog.Show(dialog, dialog.Identifier);
    }
    #endregion

    #region Logic properties
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
            typeof(StswProgressDialog)
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
            typeof(StswProgressDialog),
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
            typeof(StswProgressDialog)
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
            typeof(StswProgressDialog),
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
            typeof(StswProgressDialog),
            new FrameworkPropertyMetadata(default(CornerRadius),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswProgressDialog IsOpen="True"/>

*/
