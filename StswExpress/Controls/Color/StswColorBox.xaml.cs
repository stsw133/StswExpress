using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control that allows users to select colors either by entering color values or using a color picker and selector.
/// </summary>
[ContentProperty(nameof(SelectedColor))]
public class StswColorBox : TextBox
{
    public StswColorBox()
    {
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());
    }
    static StswColorBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorBox), new FrameworkPropertyMetadata(typeof(StswColorBox)));
    }

    #region Events
    /// <summary>
    /// Occurs when the selected color in the control changes.
    /// </summary>
    public event EventHandler? SelectedColorChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        /// Content
        if (GetTemplateChild("PART_ContentHost") is ScrollViewer content)
        {
            content.KeyDown += PART_ContentHost_KeyDown;
            content.LostFocus += PART_ContentHost_LostFocus;
        }

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Handles the KeyDown event on the content host element in the control.
    /// Triggers the LostFocus event if the Enter key is pressed.
    /// </summary>
    protected void PART_ContentHost_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            PART_ContentHost_LostFocus(sender, new RoutedEventArgs());
    }

    /// <summary>
    /// Handles the LostFocus event on the content host element in the control.
    /// Updates the selected color based on the text input and updates the source binding.
    /// </summary>
    private void PART_ContentHost_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Text))
            SelectedColor = default;
        else if (Text.Split(CultureInfo.CurrentCulture.TextInfo.ListSeparator) is string[] argb && argb.Length == 4
              && byte.TryParse(argb[0], out var a1) && byte.TryParse(argb[1], out var r1) && byte.TryParse(argb[2], out var g1) && byte.TryParse(argb[3], out var b1))
            SelectedColor = Color.FromArgb(a1, r1, g1, b1);
        else if (Text.Split(CultureInfo.CurrentCulture.TextInfo.ListSeparator) is string[] rgb && rgb.Length == 3
              && byte.TryParse(rgb[0], out var r2) && byte.TryParse(rgb[1], out var g2) && byte.TryParse(rgb[2], out var b2))
            SelectedColor = Color.FromRgb(r2, g2, b2);
        else if (new ColorConverter().IsValid(Text))
            SelectedColor = (Color)ColorConverter.ConvertFromString(Text);

        if (!IsAlphaEnabled)
            SelectedColor = Color.FromRgb(SelectedColor.R, SelectedColor.G, SelectedColor.B);

        Text = SelectedColor.ToString();
        var bindingExpression = GetBindingExpression(TextProperty);
        if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active/*, BindingStatus.UpdateSourceError*/))
            bindingExpression.UpdateSource();
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the collection of components to be displayed in the control.
    /// </summary>
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswColorBox)
        );

    /// <summary>
    /// Gets or sets the alignment of the components within the control.
    /// </summary>
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswColorBox)
        );

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
    /// Gets or sets a value indicating whether the dropdown portion of the box is open.
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
    /// Gets or sets the placeholder text to display in the box when no color is selected.
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
            typeof(StswColorBox)
        );

    /// <summary>
    /// Gets or sets the selected color in the control.
    /// </summary>
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public static readonly DependencyProperty SelectedColorProperty
        = DependencyProperty.Register(
            nameof(SelectedColor),
            typeof(Color),
            typeof(StswColorBox),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorBox stsw)
            stsw.SelectedColorChanged?.Invoke(stsw, EventArgs.Empty);
    }

    /// <summary>
    /// Gets or sets the text value of the control.
    /// </summary>
    [Browsable(false)]
    //[Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value;
    }
    #endregion

    #region Style properties
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
            typeof(StswColorBox)
        );

    /// <summary>
    /// Gets or sets the thickness of the border used as separator between box and drop-down button.
    /// </summary>
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswColorBox)
        );
    #endregion
}
