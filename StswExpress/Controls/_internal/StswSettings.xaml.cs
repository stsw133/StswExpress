using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control behaving like content dialog with various properties for customization.
/// </summary>
internal class StswSettings : Control
{
    internal StswSettings()
    {
        CloseCommand = new StswCommand<bool?>(Close);
    }
    static StswSettings()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSettings), new FrameworkPropertyMetadata(typeof(StswSettings)));
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
        if (result == true)
            Settings.Default.Save();
        else
            Settings.Default.Reload();

        StswContentDialog.Close(Window.GetWindow(this));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public static async void Show(object identifier)
    {
        StswSettings dialog = new()
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
        if (Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources) is StswResources theme)
            theme.Theme = themeID < 0 ? StswFn.GetWindowsTheme() : (StswTheme)themeID;
        Settings.Default.Theme = themeID;
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
            typeof(StswSettings)
        );
    #endregion
}
