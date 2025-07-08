using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a color input control that allows users to select colors either by entering values manually 
/// or using an integrated color picker and selector.
/// Supports alpha channel selection, dynamic color updates, and text-based color input.
/// </summary>
[ContentProperty(nameof(SelectedColor))]
[Stsw("0.1.0", Changes = StswPlannedChanges.None)]
public class StswColorBox : StswBoxBase
{
    static StswColorBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorBox), new FrameworkPropertyMetadata(typeof(StswColorBox)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the selected color in the control changes.
    /// This event is primarily for non-MVVM scenarios where direct event handling is required.
    /// </summary>
    public event EventHandler? SelectedColorChanged;

    /// <inheritdoc/>
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
    /// Gets or sets a value indicating whether the alpha channel (transparency) is enabled for color selection.
    /// When disabled, the selected color will always have full opacity.
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
    /// Gets or sets a value indicating whether the drop-down menu is currently open.
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
    /// Gets or sets the currently selected color in the control.
    /// Supports two-way binding for seamless color selection and updates.
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
        if (obj is not StswColorBox stsw)
            return;

        /// event for non MVVM programming
        stsw.SelectedColorChanged?.Invoke(stsw, new StswValueChangedEventArgs<Color?>((Color?)e.OldValue, (Color?)e.NewValue));
    }
    private static object OnSelectedColorChanging(DependencyObject d, object baseValue)
    {
        if (baseValue == null)
            return default(Color);

        return baseValue;
    }
    #endregion
}

/* usage:

<se:StswColorBox SelectedColor="{Binding BackgroundColor}" IsAlphaEnabled="False"/>

*/
