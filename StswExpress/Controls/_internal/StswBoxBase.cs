using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;
public abstract class StswBoxBase : TextBox, IStswBoxControl, IStswCornerControl
{
    public StswBoxBase()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }

    #region Events & methods
    /// <summary>
    /// Handles the KeyDown event for the internal content host of the numeric box.
    /// If the Enter key is pressed, the LostFocus event is triggered for the content host.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!AcceptsReturn && e.Key == Key.Enter)
            UpdateMainProperty(true);
    }

    /// <summary>
    /// Handles the LostFocus event for the content, updating the value and applying any necessary formatting.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(false);
        base.OnLostFocus(e);
    }

    /// <summary>
    /// Updates the main property associated with the selected value in the control based on user input.
    /// </summary>
    /// <param name="alwaysUpdate">A value indicating whether to force a binding update regardless of changes.</param>
    protected abstract void UpdateMainProperty(bool alwaysUpdate);
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a collection of errors to display in <see cref="StswSubError"/>'s tooltip.
    /// </summary>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty
        = DependencyProperty.Register(
            nameof(Errors),
            typeof(ReadOnlyObservableCollection<ValidationError>),
            typeof(StswBoxBase)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="StswSubError"/> is visible within the box when there is at least one validation error.
    /// </summary>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty
        = DependencyProperty.Register(
            nameof(HasError),
            typeof(bool),
            typeof(StswBoxBase)
        );

    /// <summary>
    /// Gets or sets the placeholder text to display in the box when no value is provided.
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
            typeof(StswBoxBase)
        );

    /// <summary>
    /// Gets or sets the collection of sub controls to be displayed in the control.
    /// </summary>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty
        = DependencyProperty.Register(
            nameof(SubControls),
            typeof(ObservableCollection<IStswSubControl>),
            typeof(StswBoxBase)
        );

    /*
    /// <summary>
    /// Gets or sets the text value of the control.
    /// </summary>
    [Browsable(false)]
    //[Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value;
    }
    */
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
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
            typeof(StswBoxBase)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
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
            typeof(StswBoxBase)
        );
    #endregion
}
