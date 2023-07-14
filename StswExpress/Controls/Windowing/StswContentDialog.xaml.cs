using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

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
    /*
    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }
    #endregion
    */
    #region Main properties
    public enum Buttons
    {
        //Custom = -1,
        OK,
        OKCancel,
        YesNoCancel,
        YesNo
    }
    public enum Images
    {
        //Custom = -1,
        None,
        Error,
        Question,
        Warning,
        Information
    }
    /// BindingModel
    public static readonly DependencyProperty BindingModelProperty
        = DependencyProperty.Register(
            nameof(BindingModel),
            typeof(StswContentDialogModel),
            typeof(StswContentDialog)
        );
    public StswContentDialogModel BindingModel
    {
        get => (StswContentDialogModel)GetValue(BindingModelProperty);
        set => SetValue(BindingModelProperty, value);
    }

    /// IsOpen
    public static readonly DependencyProperty IsOpenProperty
        = DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(StswContentDialog)
        );
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswContentDialog)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}

public class StswContentDialogModel
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public StswContentDialog.Buttons Button { get; set; } //= StswContentDialog.Buttons.Custom;
    public StswContentDialog.Images Image { get; set; } //= StswContentDialog.Images.Custom;
    public ICommand? OnYesCommand { get; set; }
    public ICommand? OnNoCommand { get; set; }
    public ICommand? OnCancelCommand { get; set; }
    public bool IsOpen { get; set; }
}