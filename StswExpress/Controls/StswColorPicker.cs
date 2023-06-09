﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StswExpress;

[ContentProperty(nameof(SelectedColor))]
public class StswColorPicker : UserControl
{
    static StswColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorPicker), new FrameworkPropertyMetadata(typeof(StswColorPicker)));
    }

    #region Events
    private Grid? partColorGrid;
    private Ellipse? partColorEllipse;
    
    /// OnApplyTemplate
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

    /// PART_ColorGrid_MouseDown
    private void PART_ColorGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            PART_ColorGrid_MouseMove(sender, e);
    }

    /// PART_ColorGrid_MouseMove
    private void PART_ColorGrid_MouseMove(object sender, MouseEventArgs e)
    {
        var grid = (Grid)sender;

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var position = e.GetPosition(grid);

            int x = (int)Math.Floor(position.X);
            int y = (int)Math.Floor(position.Y);

            if (x <= 0 || x >= (int)grid.ActualWidth || y <= 0 || y >= (int)grid.ActualHeight)
                return;

            //var rtb = new RenderTargetBitmap((int)grid.RenderSize.Width, (int)grid.RenderSize.Height, 96, 96, PixelFormats.Pbgra32);
            //rtb.Render(grid);

            //var pixels = new byte[4];
            //rtb.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);

            //PickedColor = Color.FromArgb(SelectedColor.A, pixels[2], pixels[1], pixels[0]);

            PickedColor = StswExtensions.FromHsv(x * 360 / grid.RenderSize.Width, 1 - (y / grid.RenderSize.Height), SelectedColorV);
        }
    }

    /// ColorEllipse_Move
    private void ColorEllipse_Move(Point newPosition)
    {
        if (partColorEllipse != null && newPosition.X >= 0 && newPosition.Y >= 0)
        {
            Canvas.SetLeft(partColorEllipse, newPosition.X - partColorEllipse.Width / 2);
            Canvas.SetTop(partColorEllipse, newPosition.Y - partColorEllipse.Height / 2);
        }
    }
    #endregion

    #region Main properties
    /// PickedColor
    public static readonly DependencyProperty PickedColorProperty
        = DependencyProperty.Register(
            nameof(PickedColor),
            typeof(Color),
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPickedColorChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public Color PickedColor
    {
        get => (Color)GetValue(PickedColorProperty);
        set => SetValue(PickedColorProperty, value);
    }
    public static void OnPickedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorPicker stsw)
            stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, stsw.PickedColor.R, stsw.PickedColor.G, stsw.PickedColor.B);
    }

    /// SelectedColor
    public static readonly DependencyProperty SelectedColorProperty
        = DependencyProperty.Register(
            nameof(SelectedColor),
            typeof(Color),
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorPicker stsw)
        {
            stsw.SelectedColorA = stsw.SelectedColor.A;
            stsw.SelectedColorR = stsw.SelectedColor.R;
            stsw.SelectedColorG = stsw.SelectedColor.G;
            stsw.SelectedColorB = stsw.SelectedColor.B;

            if (stsw.partColorGrid != null)
            {
                stsw.SelectedColor.ToHsv(out var h, out var s, out var v);
                stsw.ColorEllipse_Move(new Point(stsw.partColorGrid.ActualWidth * h / 360, stsw.partColorGrid.ActualHeight - (stsw.partColorGrid.ActualHeight * s)));
            }
        }
    }

    /// SelectedColorA
    public static readonly DependencyProperty SelectedColorAProperty
        = DependencyProperty.Register(
            nameof(SelectedColorA),
            typeof(byte),
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(byte),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorAChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    internal byte SelectedColorA
    {
        get => (byte)GetValue(SelectedColorAProperty);
        set => SetValue(SelectedColorAProperty, value);
    }
    public static void OnSelectedColorAChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorPicker stsw)
            stsw.SelectedColor = Color.FromArgb(stsw.SelectedColorA, stsw.SelectedColor.R, stsw.SelectedColor.G, stsw.SelectedColor.B);
    }
    /// SelectedColorR
    public static readonly DependencyProperty SelectedColorRProperty
        = DependencyProperty.Register(
            nameof(SelectedColorR),
            typeof(byte),
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(byte),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorRChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    internal byte SelectedColorR
    {
        get => (byte)GetValue(SelectedColorRProperty);
        set => SetValue(SelectedColorRProperty, value);
    }
    public static void OnSelectedColorRChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorPicker stsw)
            stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, stsw.SelectedColorR, stsw.SelectedColor.G, stsw.SelectedColor.B);
    }
    /// SelectedColorG
    public static readonly DependencyProperty SelectedColorGProperty
        = DependencyProperty.Register(
            nameof(SelectedColorG),
            typeof(byte),
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(byte),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorGChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    internal byte SelectedColorG
    {
        get => (byte)GetValue(SelectedColorGProperty);
        set => SetValue(SelectedColorGProperty, value);
    }
    public static void OnSelectedColorGChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorPicker stsw)
            stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, stsw.SelectedColor.R, stsw.SelectedColorG, stsw.SelectedColor.B);
    }
    /// SelectedColorB
    public static readonly DependencyProperty SelectedColorBProperty
        = DependencyProperty.Register(
            nameof(SelectedColorB),
            typeof(byte),
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(byte),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorBChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    internal byte SelectedColorB
    {
        get => (byte)GetValue(SelectedColorBProperty);
        set => SetValue(SelectedColorBProperty, value);
    }
    public static void OnSelectedColorBChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorPicker stsw)
            stsw.SelectedColor = Color.FromArgb(stsw.SelectedColor.A, stsw.SelectedColor.R, stsw.SelectedColor.G, stsw.SelectedColorB);
    }
    /// SelectedColorV
    public static readonly DependencyProperty SelectedColorVProperty
        = DependencyProperty.Register(
            nameof(SelectedColorV),
            typeof(double),
            typeof(StswColorPicker),
            new FrameworkPropertyMetadata(default(double),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorVChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    internal double SelectedColorV
    {
        get => (double)GetValue(SelectedColorVProperty);
        set => SetValue(SelectedColorVProperty, value);
    }
    public static void OnSelectedColorVChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorPicker stsw)
        {
            stsw.SelectedColor.ToHsv(out var h, out var s, out var v);
            stsw.SelectedColor = StswExtensions.FromAhsv(stsw.SelectedColor.A, h, s, stsw.SelectedColorV);
        }
    }
    #endregion

    #region Spacial properties
    /// > BorderThickness ...
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswColorPicker)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswColorPicker)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// > Padding ...
    /// SubPadding
    public static readonly DependencyProperty SubPaddingProperty
        = DependencyProperty.Register(
            nameof(SubPadding),
            typeof(Thickness),
            typeof(StswColorPicker)
        );
    public Thickness SubPadding
    {
        get => (Thickness)GetValue(SubPaddingProperty);
        set => SetValue(SubPaddingProperty, value);
    }
    #endregion

    #region Style properties
    /// > Opacity ...
    /// OpacityDisabled
    public static readonly DependencyProperty OpacityDisabledProperty
        = DependencyProperty.Register(
            nameof(OpacityDisabled),
            typeof(double),
            typeof(StswColorPicker)
        );
    public double OpacityDisabled
    {
        get => (double)GetValue(OpacityDisabledProperty);
        set => SetValue(OpacityDisabledProperty, value);
    }
    #endregion
}
