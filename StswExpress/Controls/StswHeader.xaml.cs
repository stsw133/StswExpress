using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

public class StswHeader : UserControl
{
    static StswHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswHeader), new FrameworkPropertyMetadata(typeof(StswHeader)));
    }

    #region Main properties
    /// ContentMargin
    public static readonly DependencyProperty ContentMarginProperty
        = DependencyProperty.Register(
            nameof(ContentMargin),
            typeof(Thickness),
            typeof(StswHeader)
        );
    public Thickness ContentMargin
    {
        get => (Thickness)GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }
    /// ContentVisibility
    public static readonly DependencyProperty ContentVisibilityProperty
        = DependencyProperty.Register(
            nameof(ContentVisibility),
            typeof(Visibility),
            typeof(StswHeader)
        );
    public Visibility ContentVisibility
    {
        get => (Visibility)GetValue(ContentVisibilityProperty);
        set => SetValue(ContentVisibilityProperty, value);
    }

    /// IconData
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswHeader)
        );
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    /// IconScale
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength?),
            typeof(StswHeader)
        );
    public GridLength? IconScale
    {
        get => (GridLength?)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    /// IconSource
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(StswHeader)
        );
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// IsBusy
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(StswHeader)
        );
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    /// Orientation
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswHeader)
        );
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > IconStrokeThickness ...
    /// IconStrokeThickness
    public static readonly DependencyProperty IconStrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(IconStrokeThickness),
            typeof(double),
            typeof(StswHeader)
        );
    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
    #endregion

    #region Style properties
    /// > IconFill ...
    /// IconFill
    public static readonly DependencyProperty IconFillProperty
        = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(StswHeader)
        );
    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }

    /// > IconStroke ...
    /// IconStroke
    public static readonly DependencyProperty IconStrokeProperty
        = DependencyProperty.Register(
            nameof(IconStroke),
            typeof(Brush),
            typeof(StswHeader)
        );
    public Brush IconStroke
    {
        get => (Brush)GetValue(IconStrokeProperty);
        set => SetValue(IconStrokeProperty, value);
    }
    #endregion
}
