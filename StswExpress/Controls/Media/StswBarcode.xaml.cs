using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

/// <summary>
/// A lightweight control that can render either a barcode or a QR-like matrix.
/// </summary>
[TemplatePart(Name = "PART_MainBorder", Type = typeof(Border))]
public class StswBarcode : Control, IStswCornerControl
{
    static StswBarcode()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswBarcode), new FrameworkPropertyMetadata(typeof(StswBarcode)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswBarcode"/> class.
    /// </summary>
    public StswBarcode()
    {
        Loaded += (_, _) => UpdateCodeImage();
        SizeChanged += (_, _) => UpdateCodeImage();
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateCodeImage();
    }

    /// <summary>
    /// Updates the generated code image based on the current properties.
    /// </summary>
    private void UpdateCodeImage()
    {
        try
        {
            var width = double.IsNaN(ActualWidth) || ActualWidth <= 0 ? 160 : ActualWidth;
            var height = double.IsNaN(ActualHeight) || ActualHeight <= 0 ? 160 : ActualHeight;

            CodeImage = CodeType switch
            {
                StswBarcodeType.QR => GenerateQrImage(width, height),
                _ => GenerateBarcodeImage(width, height)
            };
        }
        catch
        {
            CodeImage = null;
        }
    }

    /// <summary>
    /// Generates a barcode image based on the current properties.
    /// </summary>
    /// <param name="width">The desired width of the image.</param>
    /// <param name="height">The desired height of the image.</param>
    /// <returns>The generated barcode image.</returns>
    private WriteableBitmap GenerateBarcodeImage(double width, double height)
    {
        var pattern = CodeType switch
        {
            StswBarcodeType.Code39 => Code39Encoder.BuildPattern(Value),
            StswBarcodeType.Code128 => Code128Encoder.BuildPattern(Value),
            StswBarcodeType.Ean13 => Ean13Encoder.BuildPattern(Value),
            _ => Code39Encoder.BuildPattern(Value)
        };
        var moduleWidth = Math.Max(1, (int)Math.Floor(width / pattern.Count));
        var imageWidth = Math.Max(moduleWidth * pattern.Count, 1);
        var imageHeight = Math.Max((int)Math.Floor(height), 1);

        var bitmap = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Pbgra32, null);
        var dark = GetColorFromBrush(DarkBrush);
        var light = GetColorFromBrush(LightBrush);

        bitmap.Lock();
        unsafe
        {
            var buffer = (byte*)bitmap.BackBuffer;
            var stride = bitmap.BackBufferStride;

            for (int y = 0; y < imageHeight; y++)
            {
                var row = buffer + y * stride;
                var column = 0;
                foreach (var bit in pattern)
                {
                    var color = bit ? dark : light;
                    for (int mw = 0; mw < moduleWidth; mw++)
                    {
                        var idx = column * 4;
                        row[idx] = color.B;
                        row[idx + 1] = color.G;
                        row[idx + 2] = color.R;
                        row[idx + 3] = color.A;
                        column++;
                    }
                }
            }
        }
        bitmap.AddDirtyRect(new Int32Rect(0, 0, imageWidth, imageHeight));
        bitmap.Unlock();

        return bitmap;
    }

    /// <summary>
    /// Builds a simple QR-like matrix for demonstration purposes.
    /// </summary>
    /// <param name="width">The width of the matrix.</param>
    /// <param name="height">The height of the matrix.</param>
    /// <returns>The generated QR-like matrix.</returns>
    private WriteableBitmap GenerateQrImage(double width, double height)
    {
        var text = Value ?? string.Empty;

        var matrix = QrEncoder.Encode(text);
        var modules = matrix.GetLength(0);

        var moduleSize = (int)Math.Max(1, Math.Floor(Math.Min(width, height) / (modules + 8)));
        if (moduleSize < 3)
            moduleSize = 3;

        var quiet = 4;
        var imageSize = moduleSize * (modules + 2 * quiet);
        var bitmap = new WriteableBitmap(imageSize, imageSize, 96, 96, PixelFormats.Pbgra32, null);

        var dark = GetColorFromBrush(DarkBrush);
        var light = GetColorFromBrush(LightBrush);

        bitmap.Lock();
        unsafe
        {
            var buffer = (byte*)bitmap.BackBuffer;
            var stride = bitmap.BackBufferStride;

            for (var y = 0; y < imageSize; y++)
            {
                var row = buffer + y * stride;
                var moduleY = y / moduleSize - quiet;

                for (int x = 0; x < imageSize; x++)
                {
                    var moduleX = x / moduleSize - quiet;
                    var isDark = false;

                    if (moduleX >= 0 && moduleX < modules
                     && moduleY >= 0 && moduleY < modules)
                        isDark = matrix[moduleY, moduleX];

                    var color = isDark ? dark : light;
                    var idx = x * 4;
                    row[idx + 0] = color.B;
                    row[idx + 1] = color.G;
                    row[idx + 2] = color.R;
                    row[idx + 3] = color.A;
                }
            }
        }
        bitmap.AddDirtyRect(new Int32Rect(0, 0, imageSize, imageSize));
        bitmap.Unlock();

        return bitmap;
    }

    /// <summary>
    /// Gets the color from a given brush.
    /// </summary>
    /// <param name="brush">The brush to extract the color from.</param>
    /// <returns>The extracted color.</returns>
    private Color GetColorFromBrush(Brush? brush)
    {
        return brush switch
        {
            SolidColorBrush solid => solid.Color,
            null => Colors.Black,
            _ => Colors.Black
        };
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets the generated image used by the template.
    /// </summary>
    public ImageSource? CodeImage
    {
        get => (ImageSource?)GetValue(CodeImageProperty);
        private set => SetValue(CodeImagePropertyKey, value);
    }
    private static readonly DependencyPropertyKey CodeImagePropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(CodeImage),
            typeof(ImageSource),
            typeof(StswBarcode),
            new PropertyMetadata(null)
        );
    public static readonly DependencyProperty CodeImageProperty = CodeImagePropertyKey.DependencyProperty;
    private static void OnParametersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswBarcode stsw)
            return;

        stsw.UpdateCodeImage();
    }

    /// <summary>
    /// Gets or sets the type of code that should be generated.
    /// </summary>
    public StswBarcodeType CodeType
    {
        get => (StswBarcodeType)GetValue(CodeTypeProperty);
        set => SetValue(CodeTypeProperty, value);
    }
    public static readonly DependencyProperty CodeTypeProperty
        = DependencyProperty.Register(
            nameof(CodeType),
            typeof(StswBarcodeType),
            typeof(StswBarcode),
            new PropertyMetadata(default(StswBarcodeType), OnParametersChanged)
        );

    /// <summary>
    /// Gets or sets the text that will be encoded into the graphic representation.
    /// </summary>
    public string? Value
    {
        get => (string?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(StswBarcode),
            new PropertyMetadata(default(string?), OnParametersChanged)
        );
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
            typeof(StswBarcode),
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
            typeof(StswBarcode),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the brush used to paint the dark modules.
    /// </summary>
    public Brush? DarkBrush
    {
        get => (Brush?)GetValue(DarkBrushProperty);
        set => SetValue(DarkBrushProperty, value);
    }
    public static readonly DependencyProperty DarkBrushProperty
        = DependencyProperty.Register(
            nameof(DarkBrush),
            typeof(Brush),
            typeof(StswBarcode),
            new PropertyMetadata(default(Brush?), OnParametersChanged)
        );

    /// <summary>
    /// Gets or sets the brush used to paint the light modules.
    /// </summary>
    public Brush? LightBrush
    {
        get => (Brush?)GetValue(LightBrushProperty);
        set => SetValue(LightBrushProperty, value);
    }
    public static readonly DependencyProperty LightBrushProperty
        = DependencyProperty.Register(
            nameof(LightBrush),
            typeof(Brush),
            typeof(StswBarcode),
            new PropertyMetadata(default(Brush?), OnParametersChanged)
        );
    #endregion
}