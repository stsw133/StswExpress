using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress.Wpf;

/// <summary>
/// A password input control rendered directly by <see cref="StswInputBoxBase"/>.
/// Supports placeholder text, show/hide password functionality, validation, selection,
/// undo/redo, clipboard paste, and the common Stsw input-box infrastructure without
/// hosting native WPF <see cref="PasswordBox"/> or <see cref="TextBox"/> controls.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswPasswordBox Password="{Binding UserPassword}" Placeholder="Enter password" ShowPassword="True"/&gt;
/// </code>
/// </example>
public class StswPasswordBox : StswInputBoxBase, IStswBoxControl, IStswCornerControl
{
    private const char PasswordMaskCharacter = '●';
    private bool _isPasswordSynchronizationActive;

    static StswPasswordBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPasswordBox), new FrameworkPropertyMetadata(typeof(StswPasswordBox)));
    }

    public StswPasswordBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }

    #region Dependency properties

    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty =
        DependencyProperty.Register(nameof(CornerClipping), typeof(bool), typeof(StswPasswordBox));

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(StswPasswordBox));

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty =
        DependencyProperty.Register(nameof(Errors), typeof(ReadOnlyObservableCollection<ValidationError>), typeof(StswPasswordBox));

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(StswPasswordBox));

    /// <inheritdoc/>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(StswPasswordBox));

    /// <summary>
    /// Gets or sets the password value in the box.
    /// </summary>
    public string? Password
    {
        get => (string?)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(StswPasswordBox),
            CreatePasswordMetadata());

    private static FrameworkPropertyMetadata CreatePasswordMetadata()
    {
        var metadata = new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnPasswordChanged);

        metadata.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        return metadata;
    }

    private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswPasswordBox)d;
        if (input._isPasswordSynchronizationActive)
            return;

        input._isPasswordSynchronizationActive = true;
        try
        {
            input.SetCurrentValue(TextProperty, (string?)e.NewValue ?? string.Empty);
        }
        finally
        {
            input._isPasswordSynchronizationActive = false;
        }
    }

    /// <summary>
    /// Gets or sets whether the password is rendered as plain text instead of masking characters.
    /// </summary>
    public bool ShowPassword
    {
        get => (bool)GetValue(ShowPasswordProperty);
        set => SetValue(ShowPasswordProperty, value);
    }
    public static readonly DependencyProperty ShowPasswordProperty =
        DependencyProperty.Register(
            nameof(ShowPassword),
            typeof(bool),
            typeof(StswPasswordBox),
            new FrameworkPropertyMetadata(false, OnShowPasswordChanged));

    private static void OnShowPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswPasswordBox)d;
        input.InvalidateTextLayout();
        CommandManager.InvalidateRequerySuggested();
    }

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty =
        DependencyProperty.Register(nameof(SubControls), typeof(ObservableCollection<IStswSubControl>), typeof(StswPasswordBox));

    #endregion

    #region StswInputBoxBase overrides

    protected override void OnTextChanged(string oldText, string newText)
    {
        base.OnTextChanged(oldText, newText);

        if (_isPasswordSynchronizationActive)
            return;

        _isPasswordSynchronizationActive = true;
        try
        {
            SetCurrentValue(PasswordProperty, newText);
        }
        finally
        {
            _isPasswordSynchronizationActive = false;
        }
    }

    protected override string GetDisplayText(string text)
    {
        if (ShowPassword || string.IsNullOrEmpty(text))
            return base.GetDisplayText(text);

        var textElementCount = StringInfo.ParseCombiningCharacters(text).Length;
        return textElementCount == 0 ? string.Empty : new string(PasswordMaskCharacter, textElementCount);
    }

    protected override void OnCommit()
    {
        base.OnCommit();
        GetBindingExpression(PasswordProperty)?.UpdateSource();
    }

    /// <summary>
    /// A hidden password cannot be copied, cut, or dragged outside the control.
    /// Revealing it explicitly restores normal text-selection export behavior.
    /// </summary>
    protected override bool CanExportSelectedText => ShowPassword;

    protected override bool CanAcceptTextDrop(DragEventArgs e)
        => ShowPassword && base.CanAcceptTextDrop(e);

    protected internal override bool IsAutomationValueProtected => true;

    #endregion
}
