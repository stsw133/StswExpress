using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a config box behaving like content dialog.
/// </summary>
internal class StswConfig : Control
{
    public ICommand CloseCommand { get; internal set; }

    internal StswConfig()
    {
        CloseCommand = new StswCommand<bool?>(Close);
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

        /// iSize
        if (GetTemplateChild("PART_iSize") is StswSlider iSize)
            iSize.MouseLeave += (s, e) => StswSettings.Default.iSize = iSize.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    private void Close(bool? result)
    {
        StswContentDialog.Close(Window.GetWindow(this));

        if (result == true)
            StswSettings.Default.Save();
        else
        {
            StswSettings.Default.Reload();
            StswSettings.Default.iSize = StswSettings.Default.iSize;
            StswSettings.Default.Language = StswSettings.Default.Language;
            StswSettings.Default.Theme = StswSettings.Default.Theme;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public static async void Show(object identifier)
    {
        StswConfig dialog = new()
        {
            Identifier = identifier
        };
        if (!StswContentDialog.GetInstance(identifier).IsOpen)
            await StswContentDialog.Show(dialog, dialog.Identifier);
    }
    #endregion

    #region Main properties
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
            typeof(StswConfig)
        );
    #endregion

    #region Style properties
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
            typeof(StswConfig)
        );
    #endregion
}
