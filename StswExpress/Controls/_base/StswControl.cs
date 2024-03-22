using System.Windows;

namespace StswExpress;
internal class StswControl
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty IsArrowlessProperty
        = DependencyProperty.RegisterAttached(
            "IsArrowless",
            typeof(bool),
            typeof(FrameworkElement),
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
            "IsBorderless",
            typeof(bool),
            typeof(FrameworkElement),
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
}
