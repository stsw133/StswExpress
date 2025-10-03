using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a radio button control that allows the user to select a single option from a group of mutually exclusive options.
/// This control extends <see cref="RadioButton"/> with additional styling options, including corner rounding and animations.
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
public class StswRadioButton : RadioButton, IStswCornerControl
{
    private Border? _mainBorder;

    static StswRadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRadioButton), new FrameworkPropertyMetadata(typeof(StswRadioButton)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _mainBorder = GetTemplateChild("OPT_MainBorder") as Border;
    }

    /// <inheritdoc/>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        StswSharedAnimations.AnimateClick(this, _mainBorder, true);
    }

    /// <inheritdoc/>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        StswSharedAnimations.AnimateClick(this, _mainBorder, false);
    }

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
        get => (bool)GetValue(AllowUncheckProperty);
        set => SetValue(AllowUncheckProperty, value);
    }
    public static readonly DependencyProperty AllowUncheckProperty
        = DependencyProperty.Register(
            nameof(AllowUncheck),
            typeof(bool),
            typeof(StswRadioButton)
        );
    #endregion

    #region Style properties
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
            typeof(StswRadioButton),
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
            typeof(StswRadioButton),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
