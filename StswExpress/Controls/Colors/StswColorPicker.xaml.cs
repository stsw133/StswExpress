using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a color picker control that allows users to select colors using a color spectrum or hue/saturation palette.
/// Supports alpha channel selection, dynamic color updates, and precise color adjustments.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswColorPicker SelectedColor="{Binding SelectedThemeColor}" IsAlphaEnabled="True" SelectorSize="200"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedColor))]
public class StswColorPicker : Control, IStswCornerControl
{
    private FrameworkElement? _colorEllipse, _colorGrid;
    private bool _blockColorEllipse;

    static StswColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorPicker), new FrameworkPropertyMetadata(typeof(StswColorPicker)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_ColorGrid") is FrameworkElement colorGrid)
        {
            colorGrid.MouseDown += PART_ColorGrid_MouseDown;
            colorGrid.MouseMove += PART_ColorGrid_MouseMove;
            colorGrid.SizeChanged += PART_ColorGrid_SizeChanged;
            _colorGrid = colorGrid;
        }
        _colorEllipse = GetTemplateChild("PART_ColorEllipse") as FrameworkElement;
    }

    /// <summary>
    /// Handles the <see cref="UIElement.MouseDown"/> event on the color grid.
    /// Initiates color selection and triggers a position update if the left mouse button is pressed.
    /// </summary>
    /// <param name="sender">The color grid element.</param>
    /// <param name="e">The mouse event arguments.</param>
    private void PART_ColorGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            PART_ColorGrid_MouseMove(sender, e);

        e.Handled = true;
    }

    /// <summary>
    /// Handles the <see cref="UIElement.MouseMove"/> event on the color grid.
    /// Updates the selected color and adjusts the position of the selection indicator.
    /// </summary>
    /// <param name="sender">The color grid element.</param>
    /// <param name="e">The mouse event arguments.</param>
    private void PART_ColorGrid_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement grid && e.LeftButton == MouseButtonState.Pressed)
        {
            var position = e.GetPosition(grid);
            var x = Math.Floor(position.X);
            var y = Math.Floor(position.Y);

            if (x <= 0 || x >= grid.ActualWidth || y <= 0 || y >= grid.ActualHeight)
                return;

            SelectedColor = StswFnUI.ColorFromHsv(SelectedColor.A, x * 360 / grid.RenderSize.Width, 1 - (y / grid.RenderSize.Height), SelectedColorV);
            
            if (_colorEllipse != null)
            {
                Canvas.SetLeft(_colorEllipse, x - _colorEllipse.Width / 2);
                Canvas.SetTop(_colorEllipse, y - _colorEllipse.Height / 2);
            }
        }

        e.Handled = true;
    }

    /// <summary>
    /// Handles the <see cref="FrameworkElement.SizeChanged"/> event on the color grid.
    /// Updates the position of the selection indicator to reflect the current selected color.
    /// </summary>
    /// <param name="sender">The color grid element.</param>
    /// <param name="e">The event arguments containing size change details.</param>
    private void PART_ColorGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is FrameworkElement grid)
            UpdateEllipsePosition(grid);
    }

    /// <summary>
    /// Updates the position of the selection indicator (color ellipse) within the color grid
    /// based on the currently selected color's hue and saturation values.
    /// </summary>
    /// <param name="grid">The color grid element.</param>
    private void UpdateEllipsePosition(FrameworkElement grid)
    {
        StswFnUI.ColorToHsv(SelectedColor, out var h, out var s, out _);
        var x = grid.ActualWidth * h / 360;
        var y = grid.ActualHeight - (grid.ActualHeight * s);

        if (_colorEllipse != null && x >= 0 && y >= 0)
        {
            Canvas.SetLeft(_colorEllipse, x - _colorEllipse.Width / 2);
            Canvas.SetTop(_colorEllipse, y - _colorEllipse.Height / 2);
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the alpha channel (transparency) is enabled for color selection.
    /// When enabled, users can adjust the opacity of the selected color.
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
            typeof(StswColorPicker)
        );

    /// <summary>
    /// Gets or sets the picked color in the control.
    /// This represents the color selected from the color spectrum, before applying modifications.
    /// </summary>
    public Color PickedColor
    {
        get => (Color)GetValue(PickedColorProperty);
        internal set => SetValue(PickedColorProperty, value);
    }
    public static readonly DependencyProperty PickedColorProperty
        = DependencyProperty.Register(
            nameof(PickedColor),
            typeof(Color),
            typeof(StswColorPicker)
        );

    /// <summary>
    /// Gets or sets the currently selected color in the control.
    /// Supports two-way binding for real-time color adjustments.
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
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswColorPicker stsw)
            return;

        StswFnUI.ColorToHsv(stsw.SelectedColor, out var h, out var s, out var v);
        stsw.SelectedColorA = stsw.SelectedColor.A;
        stsw.SelectedColorR = stsw.SelectedColor.R;
        stsw.SelectedColorG = stsw.SelectedColor.G;
        stsw.SelectedColorB = stsw.SelectedColor.B;

        if (!stsw._blockColorEllipse)
        {
            stsw.PickedColor = StswFnUI.ColorFromHsv(h, s, 1);
            if (stsw._colorGrid != null)
                stsw.UpdateEllipsePosition(stsw._colorGrid);
        }

        stsw.SelectedColorV = v;
        stsw._blockColorEllipse = false;
    }

    /// <summary>
    /// Gets or sets the alpha (transparency) component of the selected color.
    /// This property is internally updated whenever <see cref="SelectedColor"/> changes.
    /// </summary>
    internal byte SelectedColorA
    {
        get => (byte)GetValue(SelectedColorAProperty);
        set => SetValue(SelectedColorAProperty, value);
    }
    public static readonly DependencyProperty SelectedColorAProperty
        = DependencyProperty.Register(
            nameof(SelectedColorA),
            typeof(byte),
            typeof(StswColorPicker),
            new PropertyMetadata(default(byte), OnSelectedColorAChanged)
        );
    public static void OnSelectedColorAChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswColorPicker stsw)
            return;

        stsw.SelectedColor = Color.FromArgb((byte)e.NewValue, stsw.SelectedColor.R, stsw.SelectedColor.G, stsw.SelectedColor.B);
    }

    /// <summary>
    /// Gets or sets the red component of the selected color.
    /// This property is internally updated whenever <see cref="SelectedColor"/> changes.
    /// </summary>
    internal byte SelectedColorR
    {
        get => (byte)GetValue(SelectedColorRProperty);
        set => SetValue(SelectedColorRProperty, value);
    }
    public static readonly DependencyProperty SelectedColorRProperty
        = DependencyProperty.Register(
            nameof(SelectedColorR),
            typeof(byte),
            typeof(StswColorPicker),
            new PropertyMetadata(default(byte), OnSelectedColorRChanged)
        );
    public static void OnSelectedColorRChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswColorPicker stsw)
            return;

        stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, (byte)e.NewValue, stsw.SelectedColor.G, stsw.SelectedColor.B);
    }

    /// <summary>
    /// Gets or sets the green component of the selected color.
    /// This property is internally updated whenever <see cref="SelectedColor"/> changes.
    /// </summary>

    internal byte SelectedColorG
    {
        get => (byte)GetValue(SelectedColorGProperty);
        set => SetValue(SelectedColorGProperty, value);
    }
    public static readonly DependencyProperty SelectedColorGProperty
        = DependencyProperty.Register(
            nameof(SelectedColorG),
            typeof(byte),
            typeof(StswColorPicker),
            new PropertyMetadata(default(byte), OnSelectedColorGChanged)
        );
    public static void OnSelectedColorGChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswColorPicker stsw)
            return;

        stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, stsw.SelectedColor.R, (byte)e.NewValue, stsw.SelectedColor.B);
    }

    /// <summary>
    /// Gets or sets the blue component of the selected color.
    /// This property is internally updated whenever <see cref="SelectedColor"/> changes.
    /// </summary>
    internal byte SelectedColorB
    {
        get => (byte)GetValue(SelectedColorBProperty);
        set => SetValue(SelectedColorBProperty, value);
    }
    public static readonly DependencyProperty SelectedColorBProperty
        = DependencyProperty.Register(
            nameof(SelectedColorB),
            typeof(byte),
            typeof(StswColorPicker),
            new PropertyMetadata(default(byte), OnSelectedColorBChanged)
        );
    public static void OnSelectedColorBChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswColorPicker stsw)
            return;
        
        stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, stsw.SelectedColor.R, stsw.SelectedColor.G, (byte)e.NewValue);
    }

    /// <summary>
    /// Gets or sets the value (brightness) component of the selected color in HSV color space.
    /// </summary>
    internal double SelectedColorV
    {
        get => (double)GetValue(SelectedColorVProperty);
        set => SetValue(SelectedColorVProperty, value);
    }
    public static readonly DependencyProperty SelectedColorVProperty
        = DependencyProperty.Register(
            nameof(SelectedColorV),
            typeof(double),
            typeof(StswColorPicker),
            new PropertyMetadata(default(double), OnSelectedColorVChanged)
        );
    public static void OnSelectedColorVChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswColorPicker stsw)
            return;

        StswFnUI.ColorToHsv(stsw.PickedColor, out var h, out var s, out var _);
        stsw.SelectedColor = StswFnUI.ColorFromHsv(stsw.SelectedColor.A, h, s, stsw.SelectedColorV);
        stsw._blockColorEllipse = true;
    }
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
            typeof(StswColorPicker),
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
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the minimum height and width of the color selection grid.
    /// This value determines the size of the color selection area within the control.
    /// </summary>
    public double SelectorSize
    {
        get => (double)GetValue(SelectorSizeProperty);
        set => SetValue(SelectorSizeProperty, value);
    }
    public static readonly DependencyProperty SelectorSizeProperty
        = DependencyProperty.Register(
            nameof(SelectorSize),
            typeof(double),
            typeof(StswColorPicker)
        );
    #endregion
}
