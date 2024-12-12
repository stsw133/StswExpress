using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a control that allows users to select colors from a color spectrum or hue/saturation palette.
/// </summary>
[ContentProperty(nameof(SelectedColor))]
public class StswColorPicker : Control, IStswCornerControl
{
    static StswColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorPicker), new FrameworkPropertyMetadata(typeof(StswColorPicker)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswColorPicker), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private bool _blockColorEllipse;
    private FrameworkElement? _colorEllipse;
    private FrameworkElement? _colorGrid;

    /// <summary>
    /// Occurs when the selected color in the control changes.
    /// </summary>
    public event EventHandler? SelectedColorChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// PART_ColorGrid
        if (GetTemplateChild("PART_ColorGrid") is FrameworkElement colorGrid)
        {
            colorGrid.MouseDown += PART_ColorGrid_MouseDown;
            colorGrid.MouseMove += PART_ColorGrid_MouseMove;
            colorGrid.SizeChanged += PART_ColorGrid_SizeChanged;
            _colorGrid = colorGrid;
        }
        /// PART_ColorEllipse
        if (GetTemplateChild("PART_ColorEllipse") is FrameworkElement colorEllipse)
            _colorEllipse = colorEllipse;
    }

    /// <summary>
    /// Handles the MouseDown event on the color grid element in the control.
    /// Triggers the MouseMove event if the left mouse button is pressed.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ColorGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            PART_ColorGrid_MouseMove(sender, e);

        e.Handled = true;
    }

    /// <summary>
    /// Handles the MouseMove event on the color grid element in the control.
    /// Updates the selected color and the position of the color ellipse.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ColorGrid_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement grid && e.LeftButton == MouseButtonState.Pressed)
        {
            var position = e.GetPosition(grid);
            var x = Math.Floor(position.X);
            var y = Math.Floor(position.Y);

            if (x <= 0 || x >= grid.ActualWidth || y <= 0 || y >= grid.ActualHeight)
                return;

            SelectedColor = StswFn.ColorFromHsv(SelectedColor.A, x * 360 / grid.RenderSize.Width, 1 - (y / grid.RenderSize.Height), SelectedColorV);
            
            if (_colorEllipse != null)
            {
                Canvas.SetLeft(_colorEllipse, x - _colorEllipse.Width / 2);
                Canvas.SetTop(_colorEllipse, y - _colorEllipse.Height / 2);
            }
        }

        e.Handled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ColorGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is FrameworkElement grid)
            UpdateEllipsePosition(grid);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="grid"></param>
    private void UpdateEllipsePosition(FrameworkElement grid)
    {
        StswFn.ColorToHsv(SelectedColor, out var h, out var s, out _);
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
            typeof(StswColorPicker)
        );

    /// <summary>
    /// Gets or sets the picked color in the control.
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
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorPicker stsw)
        {
            StswFn.ColorToHsv(stsw.SelectedColor, out var h, out var s, out var v);
            stsw.SelectedColorA = stsw.SelectedColor.A;
            stsw.SelectedColorR = stsw.SelectedColor.R;
            stsw.SelectedColorG = stsw.SelectedColor.G;
            stsw.SelectedColorB = stsw.SelectedColor.B;

            if (!stsw._blockColorEllipse)
            {
                stsw.PickedColor = StswFn.ColorFromHsv(h, s, 1);
                if (stsw._colorGrid != null)
                    stsw.UpdateEllipsePosition(stsw._colorGrid);
            }

            stsw.SelectedColorV = v;
            stsw._blockColorEllipse = false;

            /// event for non MVVM programming
            stsw.SelectedColorChanged?.Invoke(stsw, new StswValueChangedEventArgs<Color?>((Color?)e.OldValue, (Color?)e.NewValue));
        }
    }

    /// <summary>
    /// Gets or sets the Alpha channel of selected color.
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
        if (obj is StswColorPicker stsw)
            stsw.SelectedColor = Color.FromArgb((byte)e.NewValue, stsw.SelectedColor.R, stsw.SelectedColor.G, stsw.SelectedColor.B);
    }

    /// <summary>
    /// Gets or sets the Red channel of selected color.
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
        if (obj is StswColorPicker stsw)
            stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, (byte)e.NewValue, stsw.SelectedColor.G, stsw.SelectedColor.B);
    }

    /// <summary>
    /// Gets or sets the Green channel of selected color.
    /// </summary>
    public static void OnSelectedColorGChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorPicker stsw)
            stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, stsw.SelectedColor.R, (byte)e.NewValue, stsw.SelectedColor.B);
    }
    public static readonly DependencyProperty SelectedColorGProperty
        = DependencyProperty.Register(
            nameof(SelectedColorG),
            typeof(byte),
            typeof(StswColorPicker),
            new PropertyMetadata(default(byte), OnSelectedColorGChanged)
        );
    internal byte SelectedColorG
    {
        get => (byte)GetValue(SelectedColorGProperty);
        set => SetValue(SelectedColorGProperty, value);
    }

    /// <summary>
    /// Gets or sets the Blue channel of selected color.
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
        if (obj is StswColorPicker stsw)
            stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, stsw.SelectedColor.R, stsw.SelectedColor.G, (byte)e.NewValue);
    }

    /// <summary>
    /// Gets or sets the Value channel of selected color.
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
        if (obj is StswColorPicker stsw)
        {
            StswFn.ColorToHsv(stsw.PickedColor, out var h, out var s, out var _);
            stsw.SelectedColor = StswFn.ColorFromHsv(stsw.SelectedColor.A, h, s, stsw.SelectedColorV);
            stsw._blockColorEllipse = true;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
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
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the minimum height and width of color selection grid.
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
