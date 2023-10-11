using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Represents a control that combines the functionality of a regular button with a drop-down button.
/// </summary>
[ContentProperty(nameof(Items))]
public class StswSplitButton : ItemsControl, ICommandSource
{
    public StswSplitButton()
    {
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnPreviewMouseDownOutsideCapturedElement);
    }
    static StswSplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSplitButton), new FrameworkPropertyMetadata(typeof(StswSplitButton)));
    }

    #region Main properties
    /// <summary>
    /// Gets or sets the command associated with the control.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(StswSplitButton)
        );

    /// <summary>
    /// Gets or sets the parameter to pass to the command associated with the control.
    /// </summary>
    public object? CommandParameter
    {
        get => (object?)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(StswSplitButton)
        );

    /// <summary>
    /// Gets or sets the target element on which to execute the command associated with the control.
    /// </summary>
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }
    public static readonly DependencyProperty CommandTargetProperty
        = DependencyProperty.Register(
            nameof(CommandTarget),
            typeof(IInputElement),
            typeof(StswSplitButton)
        );

    /*
    /// <summary>
    /// Gets or sets the collection of components to be displayed in the control.
    /// </summary>
    public ObservableCollection<IStswComponent> Components
    {
        get => (ObservableCollection<IStswComponent>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<IStswComponent>),
            typeof(StswSplitButton)
        );
    */

    /// <summary>
    /// Gets or sets the alignment of the components within the control.
    /// </summary>
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswSplitButton)
        );

    /// <summary>
    /// Gets or sets the header content of the control.
    /// </summary>
    public object? Header
    {
        get => (object?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(StswSplitButton)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the drop-down portion of the button is open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswSplitButton),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsDropDownOpenChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
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

    /// <summary>
    /// Gets or sets a value indicating whether the drop button is in read-only mode.
    /// When set to true, the popup with items is accessible, but all items within the popup are disabled.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswSplitButton)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded (including popup).
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswSplitButton)
        );

    /// <summary>
    /// Gets or sets the maximum height of the drop-down portion of the button.
    /// </summary>
    public double? MaxDropDownHeight
    {
        get => (double?)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownHeightProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double?),
            typeof(StswSplitButton)
        );

    /// <summary>
    /// Gets or sets the border thickness of the drop-down popup.
    /// </summary>
    public Thickness PopupThickness
    {
        get => (Thickness)GetValue(PopupThicknessProperty);
        set => SetValue(PopupThicknessProperty, value);
    }
    public static readonly DependencyProperty PopupThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupThickness),
            typeof(Thickness),
            typeof(StswSplitButton)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between arrow icon and main button.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswSplitButton)
        );
    #endregion
}
