using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// A secure password input control that hides the entered text.
/// Supports placeholder text, show/hide password functionality, and validation.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswPasswordBox Password="{Binding UserPassword}" Placeholder="Enter password" ShowPassword="True"/&gt;
/// </code>
/// </example>
[TemplatePart(Name = PART_PasswordBox, Type = typeof(PasswordBox))]
[TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
public class StswPasswordBox : Control, IStswBoxControl, IStswCornerControl
{
    private const string PART_PasswordBox = "PART_PasswordBox";
    private const string PART_TextBox = "OPT_TextBox";

    private bool _isPasswordChanging;
    private PasswordBox? _passwordBox;
    private TextBox? _textBox;

    public StswPasswordBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswPasswordBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPasswordBox), new FrameworkPropertyMetadata(typeof(StswPasswordBox)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        if (_passwordBox != null)
            _passwordBox.PasswordChanged -= PART_PasswordBox_PasswordChanged;

        base.OnApplyTemplate();

        _passwordBox = GetTemplateChild(PART_PasswordBox) as PasswordBox;
        _textBox = GetTemplateChild(PART_TextBox) as TextBox;

        if (_passwordBox != null)
            _passwordBox.PasswordChanged += PART_PasswordBox_PasswordChanged;

        _textBox?.SetCurrentValue(TextBoxBase.IsReadOnlyProperty, IsReadOnly);

        UpdatePasswordBoxes();
        UpdateInputStates();

        if (IsKeyboardFocusWithin || IsFocused)
            Dispatcher.BeginInvoke(FocusActiveInput);
    }
    /// <inheritdoc/>
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);

        if (e.NewFocus == this)
        {
            FocusActiveInput();
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsEnabledProperty)
            UpdateInputStates();
    }

    /// <summary>
    /// Handles the PasswordChanged event of the internal <see cref="PasswordBox"/>. 
    /// Synchronizes the Password dependency property with the <see cref="PasswordBox"/>'s actual value.
    /// </summary>
    /// <param name="sender">The <see cref="PasswordBox"/> instance that triggered the event.</param>
    /// <param name="e">The event arguments.</param>
    private void PART_PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _isPasswordChanging = true;
        if (sender is PasswordBox passwordBox)
            Password = passwordBox.Password;
        _isPasswordChanging = false;
    }

    /// <summary>
    /// Sets focus to the currently active input control based on the ShowPassword property.
    /// </summary>
    private void FocusActiveInput()
    {
        if (!IsEnabled)
            return;

        if (ShowPassword)
            _textBox?.Focus();
        else if (_passwordBox?.IsEnabled == true)
            _passwordBox.Focus();
    }

    /// <summary>
    /// Updates the enabled and read-only states of the internal input controls based on the current properties.
    /// </summary>
    private void UpdateInputStates()
    {
        if (_passwordBox != null)
        {
            var isEnabled = IsEnabled && !IsReadOnly;
            _passwordBox.SetCurrentValue(IsEnabledProperty, isEnabled);
            _passwordBox.SetCurrentValue(UIElement.IsHitTestVisibleProperty, isEnabled);
        }
        _textBox?.SetCurrentValue(TextBoxBase.IsReadOnlyProperty, IsReadOnly);
    }

    /// <summary>
    /// Updates the internal password boxes to reflect the current Password property value.
    /// </summary>
    private void UpdatePasswordBoxes()
    {
        var password = Password ?? string.Empty;

        if (_passwordBox != null && !_isPasswordChanging && _passwordBox.Password != password)
        {
            _isPasswordChanging = true;
            _passwordBox.Password = password;
            _isPasswordChanging = false;
        }

        if (_textBox != null && _textBox.Text != password)
            _textBox.SetCurrentValue(TextBox.TextProperty, password);
    }
    #endregion

    #region Logic properties
    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty = StswBoxBase.ErrorsProperty.AddOwner(typeof(StswPasswordBox));

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty = StswBoxBase.HasErrorProperty.AddOwner(typeof(StswPasswordBox));

    /// <inheritdoc/>
    public object? Icon
    {
        get => (object?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty = StswBoxBase.IconProperty.AddOwner(typeof(StswPasswordBox));

    /// <inheritdoc/>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = TextBoxBase.IsReadOnlyProperty.AddOwner(
            typeof(StswPasswordBox),
            new FrameworkPropertyMetadata(default(bool), OnIsReadOnlyChanged)
        );
    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswPasswordBox stsw)
            return;

        stsw.UpdateInputStates();
    }

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
    public static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswPasswordBox stsw)
            return;

        stsw.UpdatePasswordBoxes();
    }

    /// <inheritdoc/>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty = StswBoxBase.PlaceholderProperty.AddOwner(typeof(StswPasswordBox));

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
            typeof(StswPasswordBox),
            new FrameworkPropertyMetadata(default(bool), OnShowPasswordChanged)
        );
    private static void OnShowPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswPasswordBox stsw)
            return;

        if (stsw.IsKeyboardFocusWithin || stsw.IsFocused)
            stsw.Dispatcher.BeginInvoke(stsw.FocusActiveInput);
    }

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty = StswBoxBase.SubControlsProperty.AddOwner(typeof(StswPasswordBox));
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty = StswBoxBase.CornerClippingProperty.AddOwner(typeof(StswPasswordBox));

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty = StswBoxBase.CornerRadiusProperty.AddOwner(typeof(StswPasswordBox));
    #endregion
}
