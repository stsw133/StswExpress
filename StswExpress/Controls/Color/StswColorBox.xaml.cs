﻿using System;
using System.Collections.ObjectModel;
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
        SetValue(ComponentsProperty, new ObservableCollection<IStswComponent>());
    }
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
    /// Handles the KeyDown event on the content host element in the control.
    /// Triggers the LostFocus event if the Enter key is pressed.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Enter)
            UpdateMainProperty(true);
    }

    /// <summary>
    /// Handles the LostFocus event on the content host element in the control.
    /// Updates the selected color based on the text input and updates the source binding.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(false);
        base.OnLostFocus(e);
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateMainProperty(bool alwaysUpdate)
    {
        var result = SelectedColor;

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
            Text = result.ToString();

            var bindingExpression = GetBindingExpression(TextProperty);
            if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active/*, BindingStatus.UpdateSourceError*/))
                bindingExpression.UpdateSource();
        }
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
        {
            stsw.SelectedColorChanged?.Invoke(stsw, EventArgs.Empty);
        }
    }

    ///// <summary>
    ///// Gets or sets the text value of the control.
    ///// </summary>
    //[Browsable(false)]
    ////[Bindable(false)]
    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //[EditorBrowsable(EditorBrowsableState.Never)]
    //public new string? Text
    //{
    //    get => base.Text;
    //    internal set => base.Text = value;
    //}
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
    /// Gets or sets the thickness of the separator between box and drop-down button.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswColorBox)
        );
    #endregion
}
