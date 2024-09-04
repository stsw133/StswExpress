using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Provides a base class for text-based input controls with enhanced features such as error handling,
/// placeholder text, and sub-controls. This class serves as the foundation for custom text box controls
/// with additional functionality and styling options.
/// </summary>
public abstract class StswBoxBase : TextBox, IStswBoxControl, IStswCornerControl
{
    public StswBoxBase()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }

    #region Events & methods
    /// <summary>
    /// Handles the KeyDown event for the internal content host of the text box.
    /// If the Enter key is pressed, it triggers the LostFocus event to update the main property value,
    /// allowing for immediate processing of user input.
    /// </summary>
    /// <param name="e">The event arguments associated with the KeyDown event.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!AcceptsReturn && e.Key == Key.Enter)
            UpdateMainProperty(true);
    }

    /// <summary>
    /// Handles the LostFocus event for the content of the text box, updating the main property value
    /// and applying any necessary formatting based on the user input and current settings.
    /// </summary>
    /// <param name="e">The event arguments associated with the LostFocus event.</param>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(false);
        base.OnLostFocus(e);
    }

    /// <summary>
    /// Updates the main property associated with the control based on the current value entered by the user.
    /// This method is responsible for ensuring that the value is properly validated and formatted.
    /// </summary>
    /// <param name="alwaysUpdate">A boolean value indicating whether to force the property update even if no changes were detected.</param>
    protected abstract void UpdateMainProperty(bool alwaysUpdate);
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a read-only collection of validation errors to display in the tooltip of the <see cref="StswSubError"/>.
    /// This collection provides detailed error messages when validation rules are violated.
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
    /// Gets or sets a value indicating whether the <see cref="StswSubError"/> is visible within the text box
    /// when there is at least one validation error. This property provides a visual indicator of input errors.
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
    /// Gets or sets the placeholder text to display in the text box when no value is provided by the user.
    /// This text provides a hint about the expected input or format for the field.
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
    /// Gets or sets the collection of sub-controls to be displayed within the text box control.
    /// Sub-controls allow for additional buttons or input fields to be integrated into the control.
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

    /// <summary>
    /// Gets or sets the text value of the control. This property is hidden from designers and editors
    /// to prevent direct manipulation, and it is internally managed to ensure correct data handling.
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
            typeof(StswBoxBase),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswBoxBase),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the text box and any function buttons.
    /// This property allows customization of the visual separation between components within the control.
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
            typeof(StswBoxBase),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
