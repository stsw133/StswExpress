using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace StswExpress.Avalonia;
/// <summary>
/// Represents a radio button control that allows the user to select a single option from a group of mutually exclusive options.
/// This control extends <see cref="RadioButton"/> with additional styling options, including corner rounding and optional uncheck support.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;StackPanel&gt;
///     &lt;se:StswRadioButton Content="Option 1" GroupName="Group1"/&gt;
///     &lt;se:StswRadioButton Content="Option 2" GroupName="Group1"/&gt;
/// &lt;/StackPanel&gt;
/// </code>
/// </example>
public class StswRadioButton : RadioButton
{
    #region Events & methods
    /// <inheritdoc/>
    protected override void OnClick()
    {
        if (AllowUncheck && IsChecked == true)
        {
            SetCurrentValue(IsCheckedProperty, false);
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
            return;
        }

        base.OnClick();
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (AllowUncheck && IsChecked == true && (e.Key == Key.Space || e.Key == Key.Enter))
        {
            SetCurrentValue(IsCheckedProperty, false);
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the radio button can be unchecked by clicking it again when it is already checked.
    /// </summary>
    public bool AllowUncheck
    {
        get => GetValue(AllowUncheckProperty);
        set => SetValue(AllowUncheckProperty, value);
    }
    public static readonly StyledProperty<bool> AllowUncheckProperty = AvaloniaProperty.Register<StswRadioButton, bool>(nameof(AllowUncheck), defaultValue: false);
    #endregion
}
