using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Provides a base class for text-based input controls with enhanced features such as error handling,
/// placeholder text, icons, and sub-controls. This class serves as the foundation for custom text box controls
/// with additional functionality and styling options.
/// </summary>
/// <remarks>
/// This control extends <see cref="TextBox"/> and adds support for validation errors, dynamic sub-controls, 
/// and customizable appearance. It is designed to be inherited by specialized text-based input components.
/// </remarks>
public abstract class StswBoxBase : TextBox, IStswBoxControl, IStswCornerControl
{
    public StswBoxBase()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswBoxBase()
    {
        //StswControl.OverrideBaseBorderThickness<StswBoxBase>(getExt: c => c.BorderThickness, setExt: (c, st) => c.BorderThickness = st);
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!AcceptsReturn && e.Key == Key.Enter)
            UpdateMainProperty(true);
    }

    /// <inheritdoc/>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(false);
        base.OnLostFocus(e);
    }

    /// <summary>
    /// Handles changes to the format of the text displayed in the control.
    /// </summary>
    /// <param name="newFormat">The new string format to apply to the text.</param>
    protected virtual void FormatChanged(string? newFormat)
    {
        if (GetBindingExpression(TextProperty)?.ParentBinding is Binding binding)
        {
            var newBinding = binding.Clone();
            newBinding.StringFormat = newFormat;
            SetBinding(TextProperty, newBinding);
        }
    }

    /// <summary>
    /// Updates the main property associated with the control based on the user's input.
    /// This method is responsible for validation and formatting before committing the value.
    /// </summary>
    /// <param name="alwaysUpdate">If set to <see langword="true"/>, the method will always update the main property, regardless of whether the value has changed.</param>
    protected abstract void UpdateMainProperty(bool alwaysUpdate);
    #endregion

    #region Logic properties
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public object? Icon
    {
        get => (object?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(StswBoxBase)
        );

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
    /// Gets or sets the text value of the control. 
    /// This property is hidden from designers and editors to prevent direct manipulation.
    /// Instead, the value is internally managed for validation and formatting.
    /// </summary>
    //[Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value;
    }
    #endregion

    #region Style properties
    /*
    /// <summary>
    /// Gets or sets the thickness of the border, including the inner separator value.
    /// </summary>
    public new StswThickness BorderThickness
    {
        get => (StswThickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    public new static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.Register(
            nameof(BorderThickness),
            typeof(StswThickness),
            typeof(StswBoxBase),
            new FrameworkPropertyMetadata(default(StswThickness),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                StswControl.CreateExtendedChangedCallback<StswBoxBase>((c, th) => c.SetCurrentValue(Control.BorderThicknessProperty, th)))
        );
    */
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
    /// This allows for customization of the visual spacing inside the control.
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
