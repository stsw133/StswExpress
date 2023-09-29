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
        SetValue(BindingModelProperty, new StswMessageDialogModel());
    }
    static StswMessageDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMessageDialog), new FrameworkPropertyMetadata(typeof(StswMessageDialog)));
    }

    #region Main properties
    /// <summary>
    /// Gets or sets the data model for StswContentDialog's binding.
    /// </summary>
    public StswMessageDialogModel BindingModel
    {
        get => (StswMessageDialogModel)GetValue(BindingModelProperty);
        set => SetValue(BindingModelProperty, value);
    }
    public static readonly DependencyProperty BindingModelProperty
        = DependencyProperty.Register(
            nameof(BindingModel),
            typeof(StswMessageDialogModel),
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

/// <summary>
/// Data model for StswMessageDialog's binding.
/// </summary>
public class StswMessageDialogModel
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

    public StswMessageDialogModel()
    {
        OnYesCommand = new StswCommand(() => IsOpen = false);
        OnNoCommand = new StswCommand(() => IsOpen = false);
        OnCancelCommand = new StswCommand(() => IsOpen = false);
    }
}
