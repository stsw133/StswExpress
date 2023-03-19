using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Globalization;
using System.Windows.Data;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswNavigationElement.xaml
/// </summary>
public partial class StswNavigationElement : RadioButton
{
    public StswNavigationElement()
    {
        InitializeComponent();
    }
    static StswNavigationElement()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationElement), new FrameworkPropertyMetadata(typeof(StswNavigationElement)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        foreach (var stsw in StswExtensions.FindVisualChildren<StswNavigationElement>(Content as DependencyObject))
            stsw.InExpander = true;

        base.OnApplyTemplate();
    }
    #endregion

    #region Properties
    /// ContextNamespace
    public static readonly DependencyProperty ContextNamespaceProperty
        = DependencyProperty.Register(
            nameof(ContextNamespace),
            typeof(string),
            typeof(StswNavigationElement)
        );
    public string ContextNamespace
    {
        get => (string)GetValue(ContextNamespaceProperty);
        set => SetValue(ContextNamespaceProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigationElement)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// CreateNewInstance
    public static readonly DependencyProperty CreateNewInstanceProperty
        = DependencyProperty.Register(
            nameof(CreateNewInstance),
            typeof(bool),
            typeof(StswNavigationElement)
        );
    public bool CreateNewInstance
    {
        get => (bool)GetValue(CreateNewInstanceProperty);
        set => SetValue(CreateNewInstanceProperty, value);
    }

    /// IconData
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswNavigationElement)
        );
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    /// IconFill
    public static readonly DependencyProperty IconFillProperty
        = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }
    /// IconScale
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength?),
            typeof(StswNavigationElement)
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
            typeof(StswNavigationElement)
        );
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// InExpander
    public static readonly DependencyProperty InExpanderProperty
        = DependencyProperty.Register(
            nameof(InExpander),
            typeof(bool),
            typeof(StswNavigationElement)
        );
    public bool InExpander
    {
        get => (bool)GetValue(InExpanderProperty);
        internal set => SetValue(InExpanderProperty, value);
    }

    /// IsBusy
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(StswNavigationElement)
        );
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    /// IsCompact
    public static readonly DependencyProperty IsCompactProperty
        = DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(StswNavigationElement)
        );
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }
    /// IsExpanded
    public static readonly DependencyProperty IsExpandedProperty
        = DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(StswNavigationElement)
        );
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// Text
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(object),
            typeof(StswNavigationElement)
        );
    public object? Text
    {
        get => (object?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    /// TextMargin
    public static readonly DependencyProperty TextMarginProperty
        = DependencyProperty.Register(
            nameof(TextMargin),
            typeof(Thickness),
            typeof(StswNavigationElement)
        );
    public Thickness TextMargin
    {
        get => (Thickness)GetValue(TextMarginProperty);
        set => SetValue(TextMarginProperty, value);
    }
    /// TextVisibility
    public static readonly DependencyProperty TextVisibilityProperty
        = DependencyProperty.Register(
            nameof(TextVisibility),
            typeof(Visibility),
            typeof(StswNavigationElement)
        );
    public Visibility TextVisibility
    {
        get => (Visibility)GetValue(TextVisibilityProperty);
        set => SetValue(TextVisibilityProperty, value);
    }
    #endregion

    #region Style
    /// StyleBrush
    public static readonly DependencyProperty StyleBrushProperty
        = DependencyProperty.Register(
            nameof(StyleBrush),
            typeof(Brush),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(Brush),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnStyleBrushChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public Brush StyleBrush
    {
        get => (Brush)GetValue(StyleBrushProperty);
        set => SetValue(StyleBrushProperty, value);
    }
    public static void OnStyleBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigationElement stsw)
        {
            if (e.NewValue.Equals(e.OldValue)) return;

            var val = (Brush)e.NewValue;
            var culture = CultureInfo.InvariantCulture;

            stsw.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_ColorBrightness.Instance.Convert(val, typeof(Brush), "0", culture).ToString()));
            stsw.BackgroundMouseOver = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_ColorBrightness.Instance.Convert(val, typeof(Brush), "?30", culture).ToString()));
            stsw.BackgroundPressed = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_ColorBrightness.Instance.Convert(val, typeof(Brush), "?40", culture).ToString()));
            stsw.BackgroundChecked = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_ColorBrightness.Instance.Convert(val, typeof(Brush), "?50", culture).ToString()));
            stsw.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_ColorBrightness.Instance.Convert(val, typeof(Brush), "?65", culture).ToString()));
            stsw.BorderBrushMouseOver = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_ColorBrightness.Instance.Convert(val, typeof(Brush), "?80", culture).ToString()));
            stsw.BorderBrushPressed = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_ColorBrightness.Instance.Convert(val, typeof(Brush), "?85", culture).ToString()));
            stsw.BorderBrushChecked = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_ColorBrightness.Instance.Convert(val, typeof(Brush), "?90", culture).ToString()));
            stsw.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_Color.Instance.Convert(val, typeof(Brush), "‼", culture).ToString()));
            stsw.ForegroundMouseOver = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_Color.Instance.Convert(val, typeof(Brush), "‼", culture).ToString()));
            stsw.ForegroundPressed = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_Color.Instance.Convert(val, typeof(Brush), "‼", culture).ToString()));
            stsw.ForegroundChecked = new SolidColorBrush((Color)ColorConverter.ConvertFromString(conv_Color.Instance.Convert(val, typeof(Brush), "‼", culture).ToString()));
        }
    }

    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }

    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// ForegroundMouseOver
    public static readonly DependencyProperty ForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ForegroundMouseOver),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
    }

    /// BackgroundPressed
    public static readonly DependencyProperty BackgroundPressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundPressed),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush BackgroundPressed
    {
        get => (Brush)GetValue(BackgroundPressedProperty);
        set => SetValue(BackgroundPressedProperty, value);
    }
    /// BorderBrushPressed
    public static readonly DependencyProperty BorderBrushPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushPressed),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush BorderBrushPressed
    {
        get => (Brush)GetValue(BorderBrushPressedProperty);
        set => SetValue(BorderBrushPressedProperty, value);
    }
    /// ForegroundPressed
    public static readonly DependencyProperty ForegroundPressedProperty
        = DependencyProperty.Register(
            nameof(ForegroundPressed),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush ForegroundPressed
    {
        get => (Brush)GetValue(ForegroundPressedProperty);
        set => SetValue(ForegroundPressedProperty, value);
    }

    /// BackgroundChecked
    public static readonly DependencyProperty BackgroundCheckedProperty
        = DependencyProperty.Register(
            nameof(BackgroundChecked),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush BackgroundChecked
    {
        get => (Brush)GetValue(BackgroundCheckedProperty);
        set => SetValue(BackgroundCheckedProperty, value);
    }
    /// BorderBrushChecked
    public static readonly DependencyProperty BorderBrushCheckedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushChecked),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush BorderBrushChecked
    {
        get => (Brush)GetValue(BorderBrushCheckedProperty);
        set => SetValue(BorderBrushCheckedProperty, value);
    }
    /// ForegroundChecked
    public static readonly DependencyProperty ForegroundCheckedProperty
        = DependencyProperty.Register(
            nameof(ForegroundChecked),
            typeof(Brush),
            typeof(StswNavigationElement)
        );
    public Brush ForegroundChecked
    {
        get => (Brush)GetValue(ForegroundCheckedProperty);
        set => SetValue(ForegroundCheckedProperty, value);
    }

    /// PopupBorderThickness
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswNavigationElement)
        );
    public Thickness PopupBorderThickness
    {
        get => (Thickness)GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswNavigationElement)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion

    private void Popup_Opened(object sender, EventArgs e)
    {
        if (StswExtensions.FindVisualAncestor<ItemsControl>(this) is ItemsControl i)
        {
            var source = i?.ItemsSource?.Cast<object>()?.ToList();
            InExpander = source?.Any(x => x.GetType().Name == nameof(StswNavigationElement)) == true;
        }
    }
}
