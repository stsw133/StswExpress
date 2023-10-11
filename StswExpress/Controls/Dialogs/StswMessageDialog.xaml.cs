using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control behaving like content dialog with various properties for customization.
/// </summary>
public class StswMessageDialog : Control
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
    /// <summary>
    /// 
    /// </summary>
    public ICommand CloseCommand { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    private void Close(string? result) => StswContentDialog.Close(Identifier, result != null ? bool.Parse(result) : null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="title"></param>
    /// <param name="buttons"></param>
    /// <param name="image"></param>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public static async Task<bool?> Show(string content, string? title = null, StswDialogButtons buttons = StswDialogButtons.OK, StswDialogImage image = StswDialogImage.None, object? identifier = null)
    {
        StswMessageDialog dialog = new()
        {
            Title = title,
            Content = content,
            Buttons = buttons,
            Image = image,
            Identifier = identifier ?? StswApp.StswWindow
        };
        return (bool?)await StswContentDialog.Show(dialog, dialog.Identifier);
    }
    #endregion

    #region Main properties
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
    /// Gets or sets the degree to which the corners of the control are rounded.
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
