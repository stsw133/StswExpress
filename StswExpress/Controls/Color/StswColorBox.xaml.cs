using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a control that allows users to select colors either by entering color values or using a color picker and selector.
/// </summary>
[ContentProperty(nameof(SelectedColor))]
public class StswColorBox : StswBoxBase
{
    static StswColorBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorBox), new FrameworkPropertyMetadata(typeof(StswColorBox)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the selected color in the control changes.
    /// </summary>
    public event EventHandler? SelectedColorChanged;

    /// <summary>
    /// Updates the main property associated with the selected color in the control based on user input.
    /// </summary>
    /// <param name="alwaysUpdate">A value indicating whether to force a binding update regardless of changes.</param>
    protected override void UpdateMainProperty(bool alwaysUpdate)
    {
        var result = SelectedColor ?? default;

        if (string.IsNullOrEmpty(Text))
            result = default;
        else if (Text.Split(CultureInfo.CurrentCulture.TextInfo.ListSeparator) is string[] argb && argb.Length == 4
              && byte.TryParse(argb[0], out var a) && byte.TryParse(argb[1], out var r) && byte.TryParse(argb[2], out var g) && byte.TryParse(argb[3], out var b))
            result = Color.FromArgb(a, r, g, b);
        else if (Text.Split(CultureInfo.CurrentCulture.TextInfo.ListSeparator) is string[] rgb && rgb.Length == 3
              && byte.TryParse(rgb[0], out r) && byte.TryParse(rgb[1], out g) && byte.TryParse(rgb[2], out b))
            result = Color.FromRgb(r, g, b);
        else if (new ColorConverter().IsValid(Text))
            result = (Color)ColorConverter.ConvertFromString(Text);

        if (!IsAlphaEnabled)
            result = Color.FromRgb(result.R, result.G, result.B);

        if (result != SelectedColor || alwaysUpdate)
        {
            SelectedColor = result;

            var bindingExpression = GetBindingExpression(TextProperty);
            if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active, BindingStatus.UpdateSourceError))
                bindingExpression.UpdateSource();
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the alpha channel is enabled for color selection.
    /// </summary>
    public bool IsAlphaEnabled
    {
        get => (bool)GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }
    public static readonly DependencyProperty IsAlphaEnabledProperty
        = DependencyProperty.Register(
            nameof(IsAlphaEnabled),
            typeof(bool),
            typeof(StswColorBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether or not the drop-down portion of the control is currently open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswColorBox)
        );

    /// <summary>
    /// Gets or sets the selected color in the control.
    /// </summary>
    public Color? SelectedColor
    {
        get => (Color?)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public static readonly DependencyProperty SelectedColorProperty
        = DependencyProperty.Register(
            nameof(SelectedColor),
            typeof(Color?),
            typeof(StswColorBox),
            new FrameworkPropertyMetadata(default(Color?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged, OnSelectedColorChanging, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorBox stsw)
        {
            stsw.SelectedColorChanged?.Invoke(stsw, EventArgs.Empty);
        }
    }
    private static object OnSelectedColorChanging(DependencyObject d, object baseValue)
    {
        if (baseValue == null)
            return default(Color);

        return baseValue;
    }
    #endregion
}
