using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

[ContentProperty(nameof(Items))]
public class StswNavigationElement : UserControl
{
    public StswNavigationElement()
    {
        SetValue(ItemsProperty, new ObservableCollection<StswNavigationElement>());
    }
    static StswNavigationElement()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationElement), new FrameworkPropertyMetadata(typeof(StswNavigationElement)));
    }

    #region Events
    private StswNavigation? stswNavi;
    
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// StswNavigation
        if (StswFn.FindVisualAncestor<StswNavigation>(this) is StswNavigation stswNavigation)
            stswNavi = stswNavigation;
        OnIsCheckedChanged(this, new DependencyPropertyChangedEventArgs());

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

    /// IsChecked
    public static readonly DependencyProperty IsCheckedProperty
        = DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCheckedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
    public static void OnIsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigationElement stsw)
        {
            if (stsw.IsChecked && stsw.stswNavi != null && stsw.ContextNamespace != null)
            {
                stsw.IsBusy = true;
                stsw.stswNavi.ChangeContext(stsw.ContextNamespace, stsw.CreateNewInstance);
                stsw.IsBusy = false;
            }
        }
    }

    /// IsCompact
    public static readonly DependencyProperty IsCompactProperty
        = DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCompactChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        internal set => SetValue(IsCompactProperty, value);
    }
    public static void OnIsCompactChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigationElement stsw)
        {
            if (stsw.IsCompact && stsw.Items.Count > 0)
                stsw.IsChecked = false;
        }
    }

    /// Items
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<StswNavigationElement>),
            typeof(StswNavigationElement)
        );
    public ObservableCollection<StswNavigationElement> Items
    {
        get => (ObservableCollection<StswNavigationElement>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
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
    /// > Background ...
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

    /// > BorderBrush ...
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

    /// > Foreground ...
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

    /// > BorderThickness ...
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

    /// > CornerRadius ...
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
    #endregion
}
