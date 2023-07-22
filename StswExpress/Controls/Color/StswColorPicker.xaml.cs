﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StswExpress;

/// <summary>
/// Represents a control that allows users to select colors from a color spectrum or hue/saturation palette.
/// </summary>
[ContentProperty(nameof(SelectedColor))]
public class StswColorPicker : UserControl
{
    static StswColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorPicker), new FrameworkPropertyMetadata(typeof(StswColorPicker)));
    }

    #region Events
    private bool blockColorEllipse;
    private Grid? partColorGrid;
    private Ellipse? partColorEllipse;

    /// <summary>
    /// Occurs when the selected color in the control changes.
    /// </summary>
    public event EventHandler? SelectedColorChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        /// PART_ColorGrid
        if (GetTemplateChild("PART_ColorGrid") is Grid grid)
        {
            grid.MouseDown += PART_ColorGrid_MouseDown;
            grid.MouseMove += PART_ColorGrid_MouseMove;
            partColorGrid = grid;
        }
        /// PART_ColorEllipse
        if (GetTemplateChild("PART_ColorEllipse") is Ellipse ellipse)
            partColorEllipse = ellipse;

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Handles the MouseDown event on the color grid element in the control.
    /// Triggers the MouseMove event if the left mouse button is pressed.
    /// </summary>
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
    private void PART_ColorGrid_MouseMove(object sender, MouseEventArgs e)
    {
        var grid = (Grid)sender;

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var position = e.GetPosition(grid);

            int x = (int)Math.Floor(position.X);
            int y = (int)Math.Floor(position.Y);

            /// cannot do this since getting mouse outisde the grid is not performing this event
            //if (x <= 0)
            //    x = 0;
            //else if (x >= (int)grid.ActualWidth)
            //    x = (int)grid.ActualWidth - 1;
            //
            //if (y <= 0)
            //    y = 0;
            //else if (y >= (int)grid.ActualHeight)
            //    y = (int)grid.ActualHeight - 1;

            if (x <= 0 || x >= (int)grid.ActualWidth || y <= 0 || y >= (int)grid.ActualHeight)
                return;

            SelectedColor = StswExtensions.FromAhsv(SelectedColor.A, x * 360 / grid.RenderSize.Width, 1 - (y / grid.RenderSize.Height), SelectedColorV);
            
            if (partColorEllipse != null)
            {
                Canvas.SetLeft(partColorEllipse, x - partColorEllipse.Width / 2);
                Canvas.SetTop(partColorEllipse, y - partColorEllipse.Height / 2);
            }
        }

        e.Handled = true;
    }
    #endregion

    #region Main properties
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
        set => SetValue(PickedColorProperty, value);
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
            stsw.SelectedColor.ToHsv(out var h, out var s, out var v);
            stsw.SelectedColorA = stsw.SelectedColor.A;
            stsw.SelectedColorR = stsw.SelectedColor.R;
            stsw.SelectedColorG = stsw.SelectedColor.G;
            stsw.SelectedColorB = stsw.SelectedColor.B;

            if (!stsw.blockColorEllipse)
            {
                stsw.PickedColor = StswExtensions.FromHsv(h, s, 1);

                if (stsw.partColorGrid != null)
                {
                    var x = stsw.partColorGrid.ActualWidth * h / 360;
                    var y = stsw.partColorGrid.ActualHeight - (stsw.partColorGrid.ActualHeight * s);

                    if (stsw.partColorEllipse != null && x >= 0 && y >= 0)
                    {
                        Canvas.SetLeft(stsw.partColorEllipse, x - stsw.partColorEllipse.Width / 2);
                        Canvas.SetTop(stsw.partColorEllipse, y - stsw.partColorEllipse.Height / 2);
                    }
                }
            }

            stsw.SelectedColorV = v;
            stsw.blockColorEllipse = false;

            stsw.SelectedColorChanged?.Invoke(stsw, EventArgs.Empty);
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
            stsw.PickedColor.ToHsv(out var h, out var s, out var _);
            stsw.SelectedColor = StswExtensions.FromAhsv(stsw.SelectedColor.A, h, s, stsw.SelectedColorV);
            stsw.blockColorEllipse = true;
        }
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
            typeof(StswColorPicker)
        );

    /// <summary>
    /// Gets or sets the thickness of the picker and channel rectangles inside the control.
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
            typeof(StswColorPicker)
        );

    /// <summary>
    /// Gets or sets the margin of the picker and channel rectangles inside the control.
    /// </summary>
    public Thickness SubPadding
    {
        get => (Thickness)GetValue(SubPaddingProperty);
        set => SetValue(SubPaddingProperty, value);
    }
    public static readonly DependencyProperty SubPaddingProperty
        = DependencyProperty.Register(
            nameof(SubPadding),
            typeof(Thickness),
            typeof(StswColorPicker)
        );
    #endregion
}
