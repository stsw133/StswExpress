using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswPasswordBox.xaml
/// </summary>
public partial class StswPasswordBox : UserControl
{
    public StswPasswordBox()
    {
        InitializeComponent();

        SetValue(ButtonsProperty, new ObservableCollection<ButtonBase>());
    }
    static StswPasswordBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPasswordBox), new FrameworkPropertyMetadata(typeof(StswPasswordBox)));
    }

    #region Style
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
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
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
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
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
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
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
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
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
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
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
    }

    /// BackgroundFocused
    public static readonly DependencyProperty BackgroundFocusedProperty
        = DependencyProperty.Register(
            nameof(BackgroundFocused),
            typeof(Brush),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundFocused
    {
        get => (Brush)GetValue(BackgroundFocusedProperty);
        set => SetValue(BackgroundFocusedProperty, value);
    }
    /// BorderBrushFocused
    public static readonly DependencyProperty BorderBrushFocusedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushFocused),
            typeof(Brush),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushFocused
    {
        get => (Brush)GetValue(BorderBrushFocusedProperty);
        set => SetValue(BorderBrushFocusedProperty, value);
    }
    /// ForegroundFocused
    public static readonly DependencyProperty ForegroundFocusedProperty
        = DependencyProperty.Register(
            nameof(ForegroundFocused),
            typeof(Brush),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundFocused
    {
        get => (Brush)GetValue(ForegroundFocusedProperty);
        set => SetValue(ForegroundFocusedProperty, value);
    }

    /// BackgroundReadOnly
    public static readonly DependencyProperty BackgroundReadOnlyProperty
        = DependencyProperty.Register(
            nameof(BackgroundReadOnly),
            typeof(Brush),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundReadOnly
    {
        get => (Brush)GetValue(BackgroundReadOnlyProperty);
        set => SetValue(BackgroundReadOnlyProperty, value);
    }

    /// ForegroundPlaceholder
    public static readonly DependencyProperty ForegroundPlaceholderProperty
        = DependencyProperty.Register(
            nameof(ForegroundPlaceholder),
            typeof(Brush),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundPlaceholder
    {
        get => (Brush)GetValue(ForegroundPlaceholderProperty);
        set => SetValue(ForegroundPlaceholderProperty, value);
    }

    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion

    #region Properties
    /// ButtonClearVisibility
    public static readonly DependencyProperty ButtonClearVisibilityProperty
        = DependencyProperty.Register(
            nameof(ButtonClearVisibility),
            typeof(Visibility),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Visibility))
        );
    public Visibility ButtonClearVisibility
    {
        get => (Visibility)GetValue(ButtonClearVisibilityProperty);
        set => SetValue(ButtonClearVisibilityProperty, value);
    }
    /// ButtonShowPasswordVisibility
    public static readonly DependencyProperty ButtonShowPasswordVisibilityProperty
        = DependencyProperty.Register(
            nameof(ButtonShowPasswordVisibility),
            typeof(Visibility),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Visibility))
        );
    public Visibility ButtonShowPasswordVisibility
    {
        get => (Visibility)GetValue(ButtonShowPasswordVisibilityProperty);
        set => SetValue(ButtonShowPasswordVisibilityProperty, value);
    }
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<ButtonBase>),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(ObservableCollection<ButtonBase>))
        );
    public ObservableCollection<ButtonBase> Buttons
    {
        get => (ObservableCollection<ButtonBase>)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }
    /// ButtonsAlignment
    public static readonly DependencyProperty ButtonsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonsAlignment),
            typeof(Dock),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(Dock))
        );
    public Dock ButtonsAlignment
    {
        get => (Dock)GetValue(ButtonsAlignmentProperty);
        set => SetValue(ButtonsAlignmentProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    
    /// IsReadOnly
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(bool))
        );
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// Placeholder
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(string?))
        );
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// Password
    public static readonly DependencyProperty PasswordProperty
        = DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(StswPasswordBox),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPasswordChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public string? Password
    {
        get => (string?)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }
    public static void OnPasswordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswPasswordBox stsw && stsw.partPasswordBox != null && !stsw.isPasswordChanging)
            stsw.partPasswordBox.Password = stsw.Password;
    }

    /// ShowPassword
    public static readonly DependencyProperty ShowPasswordProperty
        = DependencyProperty.Register(
            nameof(ShowPassword),
            typeof(bool),
            typeof(StswPasswordBox),
            new PropertyMetadata(default(bool))
        );
    public bool ShowPassword
    {
        get => (bool)GetValue(ShowPasswordProperty);
        set => SetValue(ShowPasswordProperty, value);
    }
    #endregion

    #region Events
    private bool isPasswordChanging;
    private PasswordBox partPasswordBox;
    private TextBox partTextBox;
    
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        partPasswordBox = (PasswordBox)GetTemplateChild("PART_PasswordBox");
        partTextBox = (TextBox)GetTemplateChild("PART_TextBox");

        OnPasswordChanged(this, new DependencyPropertyChangedEventArgs());

        base.OnApplyTemplate();
    }

    /// PART_ButtonClear_Click
    private void PART_ButtonClear_Click(object sender, RoutedEventArgs e)
    {
        partPasswordBox.Clear();
        partTextBox.Clear();
    }

    /// PART_PasswordBox_PasswordChanged
    private void PART_PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        isPasswordChanging = true;
        Password = ((PasswordBox)sender).Password;
        isPasswordChanging = false;
    }
    #endregion
}
