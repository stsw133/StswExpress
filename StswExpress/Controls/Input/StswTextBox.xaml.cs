using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Represents a control that can be used to display or edit unformatted text.
/// </summary>
[ContentProperty(nameof(Text))]
public class StswTextBox : TextBox, IStswCorner
{
    public StswTextBox()
    {
        SetValue(ComponentsProperty, new ObservableCollection<IStswComponent>());
    }
    static StswTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTextBox), new FrameworkPropertyMetadata(typeof(StswTextBox)));
    }

    #region Events & methods
    /// <summary>
    /// Handles the KeyDown event for the internal content host of the date picker.
    /// If the Enter key is pressed, the LostFocus event is triggered for the content host.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Enter)
            UpdateMainProperty();
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateMainProperty()
    {
        var bindingExpression = GetBindingExpression(TextProperty);
        if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active/*, BindingStatus.UpdateSourceError*/))
            bindingExpression.UpdateSource();
    }
    #endregion

    #region Main properties
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
            typeof(StswTextBox)
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
            typeof(StswTextBox)
        );

    /// <summary>
    /// Gets or sets the placeholder text to display in the box when no text is provided.
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
            typeof(StswTextBox)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// 
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswTextBox)
        );

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
            typeof(StswTextBox)
        );
    #endregion
}
