using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress.Wpf;

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
public class StswColorBox : StswInputBoxBase, IStswBoxControl, IStswCornerControl
{
    static StswColorBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorBox), new FrameworkPropertyMetadata(typeof(StswColorBox)));
    }

    public StswColorBox()
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
        DependencyProperty.Register(nameof(CornerClipping), typeof(bool), typeof(StswColorBox));

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(StswColorBox));

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty =
        DependencyProperty.Register(nameof(Errors), typeof(ReadOnlyObservableCollection<ValidationError>), typeof(StswColorBox));

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(StswColorBox));

    /// <inheritdoc/>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(StswColorBox));

    /// <summary>
    /// Gets or sets a value indicating whether the alpha channel (transparency) is enabled for color selection.
    /// When disabled, colors committed from the editing buffer always use full opacity.
    /// </summary>
    public bool IsAlphaEnabled
    {
        get => (bool)GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }
    public static readonly DependencyProperty IsAlphaEnabledProperty =
        DependencyProperty.Register(nameof(IsAlphaEnabled), typeof(bool), typeof(StswColorBox));

    /// <summary>
    /// Gets or sets a value indicating whether the drop-down menu is currently open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(StswColorBox));

    /// <summary>
    /// Gets or sets the currently selected color in the control.
    /// Supports two-way binding for seamless color selection and updates.
    /// </summary>
    public Color? SelectedColor
    {
        get => (Color?)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(
            nameof(SelectedColor),
            typeof(Color?),
            typeof(StswColorBox),
            new FrameworkPropertyMetadata(
                default(Color?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null,
                OnSelectedColorChanging,
                false,
                UpdateSourceTrigger.PropertyChanged));

    private static object OnSelectedColorChanging(DependencyObject d, object baseValue)
    {
        if (baseValue == null)
            return default(Color);

        return baseValue;
    }

    /// <summary>
    /// Gets or sets the thickness of the separator between the editable area and the drop-down button.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty =
        DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswColorBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty =
        DependencyProperty.Register(nameof(SubControls), typeof(ObservableCollection<IStswSubControl>), typeof(StswColorBox));

    /// <summary>
    /// Internal editing buffer. <see cref="SelectedColor"/> is the public semantic value of the control.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value ?? string.Empty;
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Enter is the explicit color commit path supplied by <see cref="StswInputBoxBase.CommitOnEnter"/>.
    /// </summary>
    protected override void OnCommit()
    {
        UpdateMainProperty(alwaysUpdate: true);
    }

    /// <summary>
    /// Losing focus commits a valid changed color.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(alwaysUpdate: false);
        base.OnLostFocus(e);
    }

    #endregion

    #region Logic

    /// <summary>
    /// Parses the editing buffer and commits <see cref="SelectedColor"/>.
    /// Supported forms are A,R,G,B; R,G,B; and values understood by <see cref="ColorConverter"/>.
    /// </summary>
    private void UpdateMainProperty(bool alwaysUpdate)
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
            var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var parts = Text.Split([separator], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 4
                && byte.TryParse(parts[0], out var a)
                && byte.TryParse(parts[1], out var r)
                && byte.TryParse(parts[2], out var g)
                && byte.TryParse(parts[3], out var b))
            {
                isPlain = true;
                result = Color.FromArgb(a, r, g, b);
            }
            else if (parts.Length == 3
                && byte.TryParse(parts[0], out r)
                && byte.TryParse(parts[1], out g)
                && byte.TryParse(parts[2], out b))
            {
                isPlain = true;
                result = Color.FromRgb(r, g, b);
            }
            else
            {
                var converter = new ColorConverter();
                if (converter.IsValid(Text))
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

            var textBinding = GetBindingExpression(TextProperty);
            var valueBinding = GetBindingExpression(SelectedColorProperty);

            if (!isInvalid && valueBinding?.Status == BindingStatus.Active)
                valueBinding.UpdateSource();

            if (textBinding != null && textBinding.Status is BindingStatus.Active or BindingStatus.UpdateSourceError)
            {
                if (string.IsNullOrWhiteSpace(Text) || isPlain)
                    textBinding.UpdateSource();
                else if (isInvalid && alwaysUpdate)
                    textBinding.UpdateSource();
            }
        }
    }

    #endregion
}
