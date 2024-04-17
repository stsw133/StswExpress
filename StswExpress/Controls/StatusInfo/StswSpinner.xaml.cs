using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public class StswSpinner : Control
{
    #region Constructor
    static StswSpinner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSpinner), new FrameworkPropertyMetadata(typeof(StswSpinner)));
    }
    #endregion

    #region DP
    public SpinnerType Type
    {
        get { return (SpinnerType)GetValue(TypeProperty); }
        set { SetValue(TypeProperty, value); }
    }
    public static readonly DependencyProperty TypeProperty =
        DependencyProperty.Register(nameof(Type), typeof(SpinnerType), typeof(StswSpinner), new PropertyMetadata(SpinnerType.Circle1));
    #endregion
}

public enum SpinnerType
{
    Circle1,
    Circle2,
    Dots,
}
