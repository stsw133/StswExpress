using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;
/// <summary>
/// Represents a control that allows users to enter a secured password.
/// </summary>
public class StswPasswordBox : Control, IStswBoxControl, IStswCornerControl
{
    public StswPasswordBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswPasswordBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPasswordBox), new FrameworkPropertyMetadata(typeof(StswPasswordBox)));
    }

    #region Events & methods
    private bool _isPasswordChanging;
    private PasswordBox? _passwordBox;

    /// <summary>
    /// Occurs when the password in the box changes.
    /// </summary>
    public event EventHandler? PasswordChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// PasswordBox: password changed
        if (GetTemplateChild("PART_PasswordBox") is PasswordBox passwordBox)
        {
            passwordBox.Password = Password;
            passwordBox.PasswordChanged += PART_PasswordBox_PasswordChanged;
            _passwordBox = passwordBox;
        }
    }

    /// <summary>
    /// Handles the PasswordChanged event for the internal PasswordBox, updating the Password property accordingly.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    public void PART_PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _isPasswordChanging = true;
        if (sender is PasswordBox passwordBox)
            Password = passwordBox.Password;
        _isPasswordChanging = false;
    }
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
            typeof(StswPasswordBox)
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
            typeof(StswPasswordBox)
        );

    /// <summary>
    /// Gets or sets the icon section of the box.
    /// </summary>
    public object? Icon
    {
        get => (object?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(StswPasswordBox)
        );

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
            typeof(StswPasswordBox)
        );

    /// <summary>
    /// Gets or sets the password value in the box.
    /// </summary>
    public string? Password
    {
        get => (string?)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }
    public static readonly DependencyProperty PasswordProperty
        = DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(StswPasswordBox),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPasswordChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnPasswordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswPasswordBox stsw)
        {
            if (stsw._passwordBox != null && !stsw._isPasswordChanging)
                stsw._passwordBox.Password = stsw.Password;

            stsw.PasswordChanged?.Invoke(stsw, EventArgs.Empty);
        }
    }

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
            typeof(StswPasswordBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the password is visible in plain text in the box.
    /// </summary>
    public bool ShowPassword
    {
        get => (bool)GetValue(ShowPasswordProperty);
        set => SetValue(ShowPasswordProperty, value);
    }
    public static readonly DependencyProperty ShowPasswordProperty
        = DependencyProperty.Register(
            nameof(ShowPassword),
            typeof(bool),
            typeof(StswPasswordBox)
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
            typeof(StswPasswordBox)
        );
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
            typeof(StswPasswordBox),
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
            typeof(StswPasswordBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
