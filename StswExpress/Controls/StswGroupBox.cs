using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

public class StswGroupBox : GroupBox
{
    static StswGroupBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswGroupBox), new FrameworkPropertyMetadata(typeof(StswGroupBox)));
    }
    
    #region Properties
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswGroupBox)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
    
    #region Style
    /// > BorderBrush ...
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswGroupBox)
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    
    /// > Header ...
    /// HeaderBorderThickness
    public static readonly DependencyProperty HeaderBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(HeaderBorderThickness),
            typeof(Thickness),
            typeof(StswGroupBox)
        );
    public Thickness HeaderBorderThickness
    {
        get => (Thickness)GetValue(HeaderBorderThicknessProperty);
        set => SetValue(HeaderBorderThicknessProperty, value);
    }
    /// HeaderPadding
    public static readonly DependencyProperty HeaderPaddingProperty
        = DependencyProperty.Register(
            nameof(HeaderPadding),
            typeof(Thickness),
            typeof(StswGroupBox)
        );
    public Thickness HeaderPadding
    {
        get => (Thickness)GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    /// > HeaderBackground ...
    /// HeaderBackground
    public static readonly DependencyProperty HeaderBackgroundProperty
        = DependencyProperty.Register(
            nameof(HeaderBackground),
            typeof(Brush),
            typeof(StswGroupBox)
        );
    public Brush HeaderBackground
    {
        get => (Brush)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }
    /// HeaderBackgroundDisabled
    public static readonly DependencyProperty HeaderBackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(HeaderBackgroundDisabled),
            typeof(Brush),
            typeof(StswGroupBox)
        );
    public Brush HeaderBackgroundDisabled
    {
        get => (Brush)GetValue(HeaderBackgroundDisabledProperty);
        set => SetValue(HeaderBackgroundDisabledProperty, value);
    }
    
    /// > HeaderForeground ...
    /// HeaderForegroundDisabled
    public static readonly DependencyProperty HeaderForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(HeaderForegroundDisabled),
            typeof(Brush),
            typeof(StswGroupBox)
        );
    public Brush HeaderForegroundDisabled
    {
        get => (Brush)GetValue(HeaderForegroundDisabledProperty);
        set => SetValue(HeaderForegroundDisabledProperty, value);
    }
    #endregion
}
