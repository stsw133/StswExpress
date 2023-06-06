using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

[ContentProperty(nameof(SelectedColor))]
public class StswColorPicker : UserControl
{
    static StswColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorPicker), new FrameworkPropertyMetadata(typeof(StswColorPicker)));
    }

    #region Events
    //private Border partColorRectangle;
    //private Ellipse partColorEllipse;
    
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// ColorRectangle
        if (GetTemplateChild("PART_ColorRectangle") is Border rect)
        {
            rect.MouseDown += PART_ColorRectangle_MouseDown;
            rect.MouseMove += PART_ColorRectangle_MouseMove;
            //partColorRectangle = rect;
        }/*
        if (GetTemplateChild("PART_ColorEllipse") is Ellipse ellipse)
        {
            partColorEllipse = ellipse;
            PART_ColorEllipse_Move();
        }*/

        base.OnApplyTemplate();
    }

    /// PART_ColorRectangle_MouseDown
    private void PART_ColorRectangle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            PART_ColorRectangle_MouseMove(sender, e);
    }

    /// PART_ColorRectangle_MouseMove
    private void PART_ColorRectangle_MouseMove(object sender, MouseEventArgs e)
    {
        var rect = (Border)sender;

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            /// get color from ColorRectangle pixel
            var position = e.GetPosition(rect);

            int x = (int)Math.Floor(position.X);
            int y = (int)Math.Floor(position.Y);

            if (x <= rect.BorderThickness.Left + rect.Margin.Left
             || x >= rect.ActualWidth - rect.BorderThickness.Right
             || y <= rect.BorderThickness.Top + rect.Margin.Top
             || y >= rect.ActualHeight - rect.BorderThickness.Bottom)
                return;

            var rtb = new RenderTargetBitmap((int)rect.RenderSize.Width, (int)rect.RenderSize.Height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(rect);

            var pixels = new byte[4];
            rtb.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);

            SelectedColor = Color.FromArgb(SelectedColor.A, pixels[2], pixels[1], pixels[0]);
        }
    }
    /*
    /// PART_ColorEllipse_Move
    private void PART_ColorEllipse_Move()
    {
        if (partColorRectangle != null && partColorEllipse != null)
        {
            SelectedColor.ToHsv(out var h, out var s, out var v);
            // move based on H horizontally and S vertically
        }
    }*/
    #endregion

    #region Main properties
    /*
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
        {
            stsw.SelectedColorA = stsw.PickedColor.A;
            stsw.SelectedColorR = stsw.PickedColor.R;
            stsw.SelectedColorG = stsw.PickedColor.G;
            stsw.SelectedColorB = stsw.PickedColor.B;
        }
    }
    */
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
        get => (double)GetValue(SelectedColorBProperty);
        set => SetValue(SelectedColorBProperty, value);
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
