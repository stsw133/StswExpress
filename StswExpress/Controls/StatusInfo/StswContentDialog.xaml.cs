using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control behaving like content dialog with various properties for customization.
/// </summary>
public class StswContentDialog : UserControl
{
    public StswContentDialog()
    {
        SetValue(BindingModelProperty, new StswContentDialogModel());
    }
    static StswContentDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswContentDialog), new FrameworkPropertyMetadata(typeof(StswContentDialog)));
    }

    #region Main properties
    /// <summary>
    /// Gets or sets the data model for StswContentDialog's binding.
    /// </summary>
    public StswContentDialogModel BindingModel
    {
        get => (StswContentDialogModel)GetValue(BindingModelProperty);
        set => SetValue(BindingModelProperty, value);
    }
    public static readonly DependencyProperty BindingModelProperty
        = DependencyProperty.Register(
            nameof(BindingModel),
            typeof(StswContentDialogModel),
            typeof(StswContentDialog)
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
            typeof(StswContentDialog)
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
            typeof(StswContentDialog)
        );
    #endregion
}

/// <summary>
/// Data model for StswContentDialog's binding.
/// </summary>
public class StswContentDialogModel
{
    /// <summary>
    /// Gets or sets the title of the content dialog.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the content of the content dialog.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the button configuration for the content dialog.
    /// </summary>
    public StswDialogButtons Buttons { get; set; }

    /// <summary>
    /// Gets or sets the image configuration for the content dialog.
    /// </summary>
    public StswDialogImage Image { get; set; }

    /// <summary>
    /// Gets or sets the command to be executed when the "Yes" or "OK" button is clicked.
    /// </summary>
    public ICommand? OnYesCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to be executed when the "No" button is clicked.
    /// </summary>
    public ICommand? OnNoCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to be executed when the "Cancel" button is clicked.
    /// </summary>
    public ICommand? OnCancelCommand { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the content dialog is open or not.
    /// </summary>
    public bool IsOpen { get; set; }
}