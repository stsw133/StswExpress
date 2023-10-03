using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a config box behaving like content dialog.
/// </summary>
internal class StswConfig : Control
{
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
    /// 
    /// </summary>
    public ICommand CloseCommand { get; set; }

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

    /// <summary>
    /// Event handler for changing the theme based on the clicked menu item.
    /// </summary>
    protected void ThemeClick(int themeID)
    {
        var res = StswResources.GetInstance();
        if (res != null)
            res.Theme = (StswTheme)themeID;

        StswSettings.Default.Theme = themeID;
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
            typeof(StswConfig)
        );
    #endregion
}
