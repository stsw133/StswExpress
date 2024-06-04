using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a config box behaving like content dialog.
/// </summary>
internal class StswConfig : Control, IStswCornerControl
{
    internal StswConfig(object? identifier)
    {
        this.identifier = identifier;
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

        /// iSize
        if (GetTemplateChild("PART_iSize") is Slider iSize)
            iSize.MouseLeave += (_, _) => StswSettings.Default.iSize = iSize.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    private void Close(bool? result)
    {
        StswContentDialog.Close(identifier);

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
        if (!StswContentDialog.GetInstance(identifier).IsOpen)
            await StswContentDialog.Show(new StswConfig(identifier), identifier);
    }
    private readonly object? identifier;
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
            typeof(StswConfig)
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
            typeof(StswConfig)
        );
    #endregion
}
