using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace StswExpress;
/// <summary>
/// Represents a color input control that allows users to select colors either by entering values manually 
/// or using an integrated color picker and selector.
/// Supports alpha channel selection, dynamic color updates, and text-based color input.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswColorBox SelectedColor="{Binding BackgroundColor}" IsAlphaEnabled="False"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedColor))]
public class StswColorBox : StswBoxBase
{
    static StswColorBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorBox), new FrameworkPropertyMetadata(typeof(StswColorBox)));
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void UpdateMainProperty(bool alwaysUpdate)
    {
        var isInvalid = false;
        var isPlain = false;

        var result = SelectedColor ?? default;

        if (string.IsNullOrWhiteSpace(Text))
        {
            result = default;
        }
        else
        {
            var sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var parts = Text.Split([sep], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            byte a, r, g, b;
            if (parts.Length == 4 &&
                byte.TryParse(parts[0], out a) &&
                byte.TryParse(parts[1], out r) &&
                byte.TryParse(parts[2], out g) &&
                byte.TryParse(parts[3], out b))
            {
                isPlain = true;
                result = Color.FromArgb(a, r, g, b);
            }
            else if (parts.Length == 3 &&
                     byte.TryParse(parts[0], out r) &&
                     byte.TryParse(parts[1], out g) &&
                     byte.TryParse(parts[2], out b))
            {
                isPlain = true;
                result = Color.FromRgb(r, g, b);
            }
            else
            {
                var conv = new ColorConverter();
                if (conv.IsValid(Text))
                {
                    isPlain = true;
                    result = (Color)ColorConverter.ConvertFromString(Text)!;
                }
                else
                {
                    isInvalid = true;
                }
            }
        }

        if (!IsAlphaEnabled)
            result = Color.FromRgb(result.R, result.G, result.B);

        if (result != SelectedColor || alwaysUpdate)
        {
            SelectedColor = result;

            var textBE = GetBindingExpression(TextProperty);
            var valueBE = GetBindingExpression(SelectedColorProperty);

            if (!isInvalid && valueBE?.Status == BindingStatus.Active)
                valueBE.UpdateSource();

            if (textBE != null && textBE.Status is BindingStatus.Active or BindingStatus.UpdateSourceError)
            {
                if (string.IsNullOrWhiteSpace(Text) || isPlain)
                    textBE.UpdateSource();
                else if (isInvalid && alwaysUpdate)
                    textBE.UpdateSource();
            }
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
                null, OnSelectedColorChanging, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static object OnSelectedColorChanging(DependencyObject d, object baseValue)
    {
        if (baseValue == null)
            return default(Color);

        return baseValue;
    }
    #endregion
}
