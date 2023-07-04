using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

public class StswContentDialog : UserControl
{
    public StswContentDialog()
    {
        SetValue(BindingDataProperty, new StswContentDialogModel());
    }
    static StswContentDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswContentDialog), new FrameworkPropertyMetadata(typeof(StswContentDialog)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }
    #endregion

    #region Main properties
    /// BindingData
    public static readonly DependencyProperty BindingDataProperty
        = DependencyProperty.Register(
            nameof(BindingData),
            typeof(StswContentDialogModel),
            typeof(StswContentDialog)
        );
    public StswContentDialogModel BindingData
    {
        get => (StswContentDialogModel)GetValue(BindingDataProperty);
        set => SetValue(BindingDataProperty, value);
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
    public ICommand? OnYesCommand { get; set; }
    public ICommand? OnNoCommand { get; set; }
    public ICommand? OnCancelCommand { get; set; }
    public bool IsOpen { get; set; }
}