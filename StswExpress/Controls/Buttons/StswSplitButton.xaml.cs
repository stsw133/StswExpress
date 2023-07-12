using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

[ContentProperty(nameof(Items))]
public class StswSplitButton : UserControl
{
    public StswSplitButton()
    {
        SetValue(ItemsProperty, new ObservableCollection<UIElement>());

        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnPreviewMouseDownOutsideCapturedElement);
    }
    static StswSplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSplitButton), new FrameworkPropertyMetadata(typeof(StswSplitButton)));
    }

    #region Main properties
    /// Command
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(StswSplitButton)
        );
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    /// CommandParameter
    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(StswSplitButton)
        );
    public object? CommandParameter
    {
        get => (object?)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    /// CommandTarget
    public static readonly DependencyProperty CommandTargetProperty
        = DependencyProperty.Register(
            nameof(CommandTarget),
            typeof(IInputElement),
            typeof(StswSplitButton)
        );
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    /// Components
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswSplitButton)
        );
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    /// ComponentsAlignment
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswSplitButton)
        );
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }

    /// Header
    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(StswSplitButton)
        );
    public object? Header
    {
        get => (object?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// IsDropDownOpen
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswSplitButton),
            new PropertyMetadata(default(bool), OnIsDropDownOpenChanged)
        );
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    private static void OnIsDropDownOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswSplitButton stsw)
        {
            if (stsw.IsDropDownOpen)
                _ = Mouse.Capture(stsw, CaptureMode.SubTree);
            else
                _ = Mouse.Capture(null);
        }
    }
    private void OnPreviewMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    /// IsReadOnly
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswSplitButton)
        );
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// Items
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<UIElement>),
            typeof(StswSplitButton)
        );
    public ObservableCollection<UIElement> Items
    {
        get => (ObservableCollection<UIElement>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > BorderThickness ...
    /// PopupBorderThickness
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswSplitButton)
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
            typeof(StswSplitButton)
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
            typeof(StswSplitButton)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// > Height ...
    /// MaxDropDownHeight
    public static readonly DependencyProperty MaxDropDownHeightProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double?),
            typeof(StswSplitButton)
        );
    public double? MaxDropDownHeight
    {
        get => (double?)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    #endregion
}
