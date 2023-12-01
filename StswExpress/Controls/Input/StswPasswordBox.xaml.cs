using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a control that allows users to enter a secured password.
/// </summary>
public class StswPasswordBox : Control, IStswCorner
{
    public StswPasswordBox()
    {
        SetValue(ComponentsProperty, new ObservableCollection<IStswComponent>());
    }
    static StswPasswordBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPasswordBox), new FrameworkPropertyMetadata(typeof(StswPasswordBox)));
    }

    #region Events & methods
    private bool _isPasswordChanging;
    private PasswordBox? passwordBox;

    /// <summary>
    /// Occurs when the password in the StswPasswordBox changes.
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
            this.passwordBox = passwordBox;
        }
    }

    /// <summary>
    /// Handles the PasswordChanged event for the internal PasswordBox, updating the Password property accordingly.
    /// </summary>
    public void PART_PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _isPasswordChanging = true;
        if (sender is PasswordBox passwordBox)
            Password = passwordBox.Password;
        _isPasswordChanging = false;
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
            typeof(StswPasswordBox)
        );

    /// <summary>
    /// Gets or sets the password value in the control.
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
            if (stsw.passwordBox != null && !stsw._isPasswordChanging)
                stsw.passwordBox.Password = stsw.Password;

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
    /// Gets or sets a value indicating whether the password is visible in plain text in the control.
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
            typeof(StswPasswordBox)
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
            typeof(StswPasswordBox)
        );
    #endregion
}
