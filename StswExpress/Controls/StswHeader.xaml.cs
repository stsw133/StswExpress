using DocumentFormat.OpenXml.Math;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswHeader.xaml
/// </summary>
public partial class StswHeader : StswHeaderBase
{
    public StswHeader()
    {
        InitializeComponent();
    }
    static StswHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswHeader), new FrameworkPropertyMetadata(typeof(StswHeader)));
    }
}

public class StswHeaderBase : UserControl
{
    public StswHeaderBase()
    {
        SetValue(SubTextsProperty, new ObservableCollection<UIElement>());
    }

    /// HideText
    public static readonly DependencyProperty HideTextProperty
        = DependencyProperty.Register(
            nameof(HideText),
            typeof(bool),
            typeof(StswHeaderBase),
            new PropertyMetadata(default(bool))
        );
    public bool HideText
    {
        get => (bool)GetValue(HideTextProperty);
        set => SetValue(HideTextProperty, value);
    }

    /// IconData
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswHeaderBase),
            new PropertyMetadata(default(Geometry?))
        );
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    /// IconForeground
    public static readonly DependencyProperty IconForegroundProperty
        = DependencyProperty.Register(
            nameof(IconForeground),
            typeof(Brush),
            typeof(StswHeaderBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }
    /// IconScale
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(double?),
            typeof(StswHeaderBase),
            new PropertyMetadata(default(double?))
        );
    public double? IconScale
    {
        get => (double?)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    /// IconSource
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(StswHeaderBase),
            new PropertyMetadata(default(ImageSource?))
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
            typeof(StswHeaderBase),
            new PropertyMetadata(default(bool))
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
            typeof(StswHeaderBase),
            new PropertyMetadata(default(Orientation))
        );
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// SubTexts
    public static readonly DependencyProperty SubTextsProperty
        = DependencyProperty.Register(
              nameof(SubTexts),
              typeof(ObservableCollection<UIElement>),
              typeof(StswHeaderBase),
              new PropertyMetadata(default(ObservableCollection<UIElement>))
          );
    public ObservableCollection<UIElement> SubTexts
    {
        get => (ObservableCollection<UIElement>)GetValue(SubTextsProperty);
        set => SetValue(SubTextsProperty, value);
    }

    /// TextPadding
    public static readonly DependencyProperty TextPaddingProperty
        = DependencyProperty.Register(
            nameof(TextPadding),
            typeof(Thickness),
            typeof(StswHeaderBase),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness TextPadding
    {
        get => (Thickness)GetValue(TextPaddingProperty);
        set => SetValue(TextPaddingProperty, value);
    }
}
