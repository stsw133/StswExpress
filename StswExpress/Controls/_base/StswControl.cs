using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Provides attached properties for customizing the appearance and behavior of controls.
/// </summary>
public static class StswControl
{
    /// <summary>
    /// Identifies the IsArrowless attached property.
    /// When set to <see langword="true"/>, it hides the drop-down arrow by setting its visibility to <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public static readonly DependencyProperty IsArrowlessProperty
        = DependencyProperty.RegisterAttached(
            nameof(IsArrowlessProperty)[..^8],
            typeof(bool),
            typeof(StswControl),
            new PropertyMetadata(false, OnIsArrowlessChanged)
        );
    //public static bool GetIsArrowless(DependencyObject obj) => (bool)obj.GetValue(IsArrowlessProperty);
    public static void SetIsArrowless(DependencyObject obj, bool value) => obj.SetValue(IsArrowlessProperty, value);
    private static void OnIsArrowlessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is FrameworkElement stsw)
        {
            stsw.Resources.Add("StswDropArrow.Visibility", (bool?)e.NewValue == true ? Visibility.Collapsed : Visibility.Visible);
        }
    }

    /// <summary>
    /// Attached property to control the borderless appearance of controls implementing the <see cref="IStswCornerControl"/> interface.
    /// When set to <see langword="true"/>, it hides the border by setting BorderThickness to <c>0</c>,
    /// CornerClipping to <see langword="false"/>, and CornerRadius to <c>0</c>.
    /// </summary>
    public static readonly DependencyProperty IsBorderlessProperty
        = DependencyProperty.RegisterAttached(
            nameof(IsBorderlessProperty)[..^8],
            typeof(bool),
            typeof(StswControl),
            new PropertyMetadata(false, OnIsBorderlessChanged)
        );
    //public static bool GetIsBorderless(DependencyObject obj) => (bool)obj.GetValue(IsBorderlessProperty);
    public static void SetIsBorderless(DependencyObject obj, bool value) => obj.SetValue(IsBorderlessProperty, value);
    private static void OnIsBorderlessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswCornerControl stsw)
        {
            if ((bool)e.NewValue)
            {
                stsw.BorderThickness = new(0);
                stsw.CornerClipping = false;
                stsw.CornerRadius = new(0);
            }
        }
    }

    /// <summary>
    /// Identifies the SubControlsDock attached property.
    /// Determines the docking position of sub-controls.
    /// </summary>
    public static readonly DependencyProperty SubControlsDockProperty
        = DependencyProperty.RegisterAttached(
            nameof(SubControlsDockProperty)[..^8],
            typeof(Dock),
            typeof(StswControl),
            new PropertyMetadata(Dock.Right)
        );
    public static Dock GetSubControlsDock(DependencyObject obj) => (Dock)obj.GetValue(SubControlsDockProperty);
    public static void SetSubControlsDock(DependencyObject obj, Dock value) => obj.SetValue(SubControlsDockProperty, value);
}
