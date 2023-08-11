using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that allows selection from a drop-down list of items.
/// </summary>
public class StswComboBox : ComboBox
{
    public StswComboBox()
    {
        SetValue(ComponentsProperty, new ObservableCollection<IStswComponent>());
    }
    static StswComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComboBox), new FrameworkPropertyMetadata(typeof(StswComboBox)));
    }

    #region Main properties
    /// <summary>
    /// Gets or sets the visibility of the arrow icon in the drop button.
    /// </summary>
    public Visibility ArrowVisibility
    {
        get => (Visibility)GetValue(ArrowVisibilityProperty);
        set => SetValue(ArrowVisibilityProperty, value);
    }
    public static readonly DependencyProperty ArrowVisibilityProperty
        = DependencyProperty.Register(
            nameof(ArrowVisibility),
            typeof(Visibility),
            typeof(StswComboBox)
        );

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
            typeof(StswComboBox)
        );

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
            typeof(StswComboBox)
        );

    /// <summary>
    /// Gets or sets the placeholder text displayed in the control when no item is selected.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswComboBox)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswComboBox)
        );

    /// <summary>
    /// Gets or sets the border thickness of the drop-down popup.
    /// </summary>
    public Thickness PopupBorderThickness
    {
        get => (Thickness)GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswComboBox)
        );

    /// <summary>
    /// Gets or sets the thickness of the border used as separator between arrow icon and main button.
    /// </summary>
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswComboBox)
        );
    #endregion
}

/// <summary>
/// Provides a way to store and display pairs of display and value objects for use in combo boxes.
/// </summary>
public class StswComboItem : StswObservableObject
{
    /// <summary>
    /// Gets or sets the display text for the combo box item.
    /// </summary>
    public object? Display
    {
        get => display;
        set => SetProperty(ref display, value);
    }
    private object? display;

    /// <summary>
    /// Gets or sets the value associated with the combo box item.
    /// </summary>
    public object? Value
    {
        get => value;
        set => SetProperty(ref this.value, value);
    }
    private object? value;
}
