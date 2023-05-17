using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

[ContentProperty(nameof(Items))]
public class StswButtonSelector : ToggleButton
{
    public StswButtonSelector()
    {
        SetValue(ItemsProperty, new ObservableCollection<ButtonBase>());
    }
    static StswButtonSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswButtonSelector), new FrameworkPropertyMetadata(typeof(StswButtonSelector)));
    }

    #region Properties
    /// Header
    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(StswButtonSelector)
        );
    public object? Header
    {
        get => (object?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// Items
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<ButtonBase>),
            typeof(StswButtonSelector)
        );
    public ObservableCollection<ButtonBase> Items
    {
        get => (ObservableCollection<ButtonBase>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    #endregion

    #region Style
    /// > Background ...
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BackgroundPressed
    public static readonly DependencyProperty BackgroundPressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundPressed),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush BackgroundPressed
    {
        get => (Brush)GetValue(BackgroundPressedProperty);
        set => SetValue(BackgroundPressedProperty, value);
    }
    /// PopupBackground
    public static readonly DependencyProperty PopupBackgroundProperty
        = DependencyProperty.Register(
            nameof(PopupBackground),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush PopupBackground
    {
        get => (Brush)GetValue(PopupBackgroundProperty);
        set => SetValue(PopupBackgroundProperty, value);
    }

    /// > BorderBrush ...
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// BorderBrushPressed
    public static readonly DependencyProperty BorderBrushPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushPressed),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush BorderBrushPressed
    {
        get => (Brush)GetValue(BorderBrushPressedProperty);
        set => SetValue(BorderBrushPressedProperty, value);
    }
    /// BorderBrushDefaulted
    public static readonly DependencyProperty BorderBrushDefaultedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDefaulted),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush BorderBrushDefaulted
    {
        get => (Brush)GetValue(BorderBrushDefaultedProperty);
        set => SetValue(BorderBrushDefaultedProperty, value);
    }
    /// PopupBorderBrush
    public static readonly DependencyProperty PopupBorderBrushProperty
        = DependencyProperty.Register(
            nameof(PopupBorderBrush),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush PopupBorderBrush
    {
        get => (Brush)GetValue(PopupBorderBrushProperty);
        set => SetValue(PopupBorderBrushProperty, value);
    }

    /// > Foreground ...
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }
    /// ForegroundMouseOver
    public static readonly DependencyProperty ForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ForegroundMouseOver),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
    }
    /// ForegroundPressed
    public static readonly DependencyProperty ForegroundPressedProperty
        = DependencyProperty.Register(
            nameof(ForegroundPressed),
            typeof(Brush),
            typeof(StswButtonSelector)
        );
    public Brush ForegroundPressed
    {
        get => (Brush)GetValue(ForegroundPressedProperty);
        set => SetValue(ForegroundPressedProperty, value);
    }

    /// > BorderThickness ...
    /// PopupBorderThickness
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswButtonSelector)
        );
    public Thickness PopupBorderThickness
    {
        get => (Thickness)GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswButtonSelector)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    /// PopupCornerRadius
    public static readonly DependencyProperty PopupCornerRadiusProperty
        = DependencyProperty.Register(
            nameof(PopupCornerRadius),
            typeof(CornerRadius),
            typeof(StswButtonSelector)
        );
    public CornerRadius PopupCornerRadius
    {
        get => (CornerRadius)GetValue(PopupCornerRadiusProperty);
        set => SetValue(PopupCornerRadiusProperty, value);
    }

    /// > Padding ...
    /// PopupPadding
    public static readonly DependencyProperty PopupPaddingProperty
        = DependencyProperty.Register(
            nameof(PopupPadding),
            typeof(Thickness),
            typeof(StswButtonSelector)
        );
    public Thickness PopupPadding
    {
        get => (Thickness)GetValue(PopupPaddingProperty);
        set => SetValue(PopupPaddingProperty, value);
    }
    #endregion
}
