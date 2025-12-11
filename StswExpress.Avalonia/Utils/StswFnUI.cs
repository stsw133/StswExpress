using Avalonia;using Avalonia.Media;using Avalonia.Media.Imaging;using Avalonia.Platform;using System.Reflection;using System.Runtime.InteropServices;using System.Security.Cryptography;using System.Text;namespace StswExpress.Avalonia;
/// <summary>
/// Utility class providing various helper functions for general use.
/// </summary>
public static class StswFnUI
{
    #region Assembly functions
    /// <summary>
    /// Gets the name and version number of the currently executing application for XAML bindings.
    /// </summary>
    /// <returns>A string containing the name and version number of the currently executing application.</returns>
    public static string? AppNameAndVersion => StswFn.AppVersion != "1" ? $"{StswFn.AppName} {StswFn.AppVersion}" : StswFn.AppName;

    /// <summary>
    /// Retrieves the content of an embedded asset as bytes.
    /// </summary>
    /// <param name="relativeUri">Relative or absolute URI of the resource.</param>
    /// <returns>The resource as a byte array.</returns>
    public static byte[]? GetResourceAsBytes(string relativeUri)
    {
        var uri = new Uri(relativeUri, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
            uri = new Uri($"avares://{StswFn.AppName}/{relativeUri}", UriKind.Absolute);

        using var stream = AssetLoader.Open(uri);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Retrieves the content of an embedded resource file from a specified assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly containing the resource.</param>
    /// <param name="resourcePath">The path of the resource file within the assembly.</param>
    /// <returns>The content of the resource file as a string.</returns>
    public static string? GetResourceAsText(string assemblyName, string resourcePath)
    {
        var resourceUri = new Uri($"avares://{assemblyName}/{resourcePath}", UriKind.Absolute);

        using var stream = AssetLoader.Open(resourceUri);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    #endregion

    #region Binding helpers
    /// <summary>
    /// Boolean value representing true for XAML bindings.
    /// </summary>
    public static readonly bool True = true;

    /// <summary>
    /// Boolean value representing false for XAML bindings.
    /// </summary>
    public static readonly bool False = false;

    /// <summary>
    /// Gets the current date with the time component set to 00:00:00.
    /// </summary>
    public static DateTime CurrentDate => DateTime.Today;

    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    public static DateTime CurrentDateTime => DateTime.Now;
    #endregion

    #region Color functions
    /// <summary>
    /// Creates a <see cref="Color"/> from the specified alpha, hue, saturation, and lightness (HSL) values.
    /// </summary>
    /// <param name="alpha">The alpha component (0-255).</param>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="lightness">The lightness component (0-1).</param>
    /// <returns>A <see cref="Color"/> object representing the specified HSL values.</returns>
    public static Color ColorFromHsl(byte alpha, double hue, double saturation, double lightness)
    {
        var h = hue / 360.0;
        var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        var x = c * (1 - Math.Abs((h * 6) % 2 - 1));
        var m = lightness - c / 2;

        double r = 0, g = 0, b = 0;

        if (h < 1.0 / 6) { r = c; g = x; }
        else if (h < 2.0 / 6) { r = x; g = c; }
        else if (h < 3.0 / 6) { g = c; b = x; }
        else if (h < 4.0 / 6) { g = x; b = c; }
        else if (h < 5.0 / 6) { r = x; b = c; }
        else { r = c; b = x; }

        return Color.FromArgb(
            alpha,
            (byte)Math.Round((r + m) * 255),
            (byte)Math.Round((g + m) * 255),
            (byte)Math.Round((b + m) * 255)
        );
    }

    /// <summary>
    /// Creates a <see cref="Color"/> from the specified hue, saturation, and lightness (HSL) values with full opacity.
    /// </summary>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="lightness">The lightness component (0-1).</param>
    /// <returns>A <see cref="Color"/> object representing the specified HSL values with full opacity.</returns>
    public static Color ColorFromHsl(double hue, double saturation, double lightness) => ColorFromHsl(255, hue, saturation, lightness);

    /// <summary>
    /// Converts a <see cref="Color"/> to its hue, saturation, and lightness (HSL) components.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="lightness">The lightness component (0-1).</param>
    public static void ColorToHsl(Color color, out double hue, out double saturation, out double lightness)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));

        lightness = (max + min) / 2.0;

        if (max == min)
        {
            hue = 0;
            saturation = 0;
        }
        else
        {
            var delta = max - min;

            saturation = lightness > 0.5
                ? delta / (2.0 - max - min)
                : delta / (max + min);

            if (max == r)
                hue = (g - b) / delta + (g < b ? 6 : 0);
            else if (max == g)
                hue = (b - r) / delta + 2;
            else
                hue = (r - g) / delta + 4;

            hue *= 60;
        }
    }

    /// <summary>
    /// Creates a <see cref="Color"/> from the specified alpha, hue, saturation, and value (HSV) values.
    /// </summary>
    /// <param name="alpha">The alpha component (0-255).</param>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="value">The value component (0-1).</param>
    /// <returns>A <see cref="Color"/> object representing the specified HSV values.</returns>
    public static Color ColorFromHsv(byte alpha, double hue, double saturation, double value)
    {
        hue = hue % 360;
        var h = (int)(hue / 60) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        var v = (byte)(value * 255);
        var p = (byte)(v * (1 - saturation));
        var q = (byte)(v * (1 - f * saturation));
        var t = (byte)(v * (1 - (1 - f) * saturation));

        return h switch
        {
            0 => Color.FromArgb(alpha, v, t, p),
            1 => Color.FromArgb(alpha, q, v, p),
            2 => Color.FromArgb(alpha, p, v, t),
            3 => Color.FromArgb(alpha, p, q, v),
            4 => Color.FromArgb(alpha, t, p, v),
            _ => Color.FromArgb(alpha, v, p, q)
        };
    }

    /// <summary>
    /// Creates a <see cref="Color"/> from the specified hue, saturation, and value (HSV) values with full opacity.
    /// </summary>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="value">The value component (0-1).</param>
    /// <returns>A <see cref="Color"/> object representing the specified HSV values with full opacity.</returns>
    public static Color ColorFromHsv(double hue, double saturation, double value) => ColorFromHsv(255, hue, saturation, value);

    /// <summary>
    /// Converts a <see cref="Color"/> to its hue, saturation, and value (HSV) components.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <param name="hue">The hue component (0-360).</param>
    /// <param name="saturation">The saturation component (0-1).</param>
    /// <param name="value">The value component (0-1).</param>
    public static void ColorToHsv(Color color, out double hue, out double saturation, out double value)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        if (delta == 0)
            hue = 0;
        else if (max == r)
            hue = 60 * (((g - b) / delta) % 6);
        else if (max == g)
            hue = 60 * (((b - r) / delta) + 2);
        else
            hue = 60 * (((r - g) / delta) + 4);

        if (hue < 0)
            hue += 360;

        saturation = (max == 0) ? 0 : delta / max;

        value = max;
    }

    /// <summary>
    /// Generates a deterministic color based on the SHA-256 hash of the input text.
    /// The seed value adjusts brightness to ensure contrast variations.
    /// </summary>
    /// <param name="text">The text to convert into a color.</param>
    /// <param name="seed">The seed value used to adjust the brightness of the generated color.</param>
    /// <returns>A <see cref="Color"/> object generated from the text and adjusted by the seed value.</returns>
    public static Color GenerateColor(string text, int seed)
    {
        if (string.IsNullOrEmpty(text))
            return Colors.Transparent;

        var combined = $"{seed}\u001F{text}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));

        return Color.FromArgb(255, hashBytes[0], hashBytes[1], hashBytes[2]);
    }
    #endregion

    #region Compare functions
    /// <summary>
    /// Compares two models of the same type and returns a dictionary indicating whether each property value is equal.
    /// </summary>
    /// <typeparam name="T">The type of the models to compare.</typeparam>
    /// <param name="model1">The first model to compare.</param>
    /// <param name="model2">The second model to compare.</param>
    /// <returns>The dictionary where the key is the property name and the value is a boolean indicating whether the property values are equal.</returns>
    public static Dictionary<string, bool> CompareModels<T>(T model1, T model2)
    {
        var result = new Dictionary<string, bool>();

        ArgumentNullException.ThrowIfNull(model1, nameof(model1));
        ArgumentNullException.ThrowIfNull(model2, nameof(model2));

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (!prop.CanRead) continue;

            var value1 = prop.GetValue(model1);
            var value2 = prop.GetValue(model2);

            var areEqual = Equals(value1, value2);
            result[prop.Name] = areEqual;
        }

        return result;
    }
    #endregion

    #region Convert functions
    /// <summary>
    /// Converts a byte array to a <see cref="Bitmap"/>.
    /// </summary>
    /// <param name="value">The byte array to convert.</param>
    /// <returns>The converted <see cref="Bitmap"/>, or null if the byte array is empty.</returns>
    public static Bitmap? BytesToBitmap(byte[]? value)
    {
        if (value == null || value.Length == 0)
            return null;

        using var mem = new MemoryStream(value);
        return new Bitmap(mem);
    }
    #endregion

    #region File functions
    /// <summary>
    /// Extracts the associated icon of the specified file or directory path.
    /// If the path points to a directory, attempts to retrieve the default folder icon.
    /// </summary>
    /// <param name="path">The file or directory path to extract the icon from.</param>
    /// <returns>The associated icon as an <see cref="ImageSource"/> if found; otherwise, <see langword="null"/>.</returns>
    public static IImage? ExtractAssociatedIcon(string? path, bool largeIcon = true)
    {
        if (!Path.Exists(path))
            return null;

        var flags = SHGFI_ICON | (largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON);
        if (SHGetFileInfo(path, 0, out var shinfo, (uint)Marshal.SizeOf<SHFILEINFO>(), flags) == IntPtr.Zero || shinfo.hIcon == IntPtr.Zero)
            return null;

        try
        {
            return shinfo.hIcon.ToAvaloniaBitmap();
        }
        finally
        {
            DestroyIcon(shinfo.hIcon);
        }
    }

    /// <summary>
    /// Converts a Windows icon handle (HICON) to an Avalonia <see cref="IImage"/>.
    /// </summary>
    /// <param name="hIcon">The handle to the icon (HICON).</param>
    /// <returns>The converted Avalonia <see cref="IImage"/>.</returns>
    private static WriteableBitmap? ToAvaloniaBitmap(this IntPtr hIcon)
    {
        if (hIcon == IntPtr.Zero)
            return null;

        if (!GetIconInfo(hIcon, out var iconInfo))
            return null;

        var hbmColor = iconInfo.hbmColor;
        var hbmMask = iconInfo.hbmMask;

        try
        {
            if (hbmColor == IntPtr.Zero)
                return null;

            var bmi = new BITMAPINFO();
            bmi.bmiHeader.biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>();

            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
                return null;

            try
            {
                if (GetDIBits(hdc, hbmColor, 0, 0, IntPtr.Zero, ref bmi, DIB_RGB_COLORS) == 0)
                    return null;

                int width = bmi.bmiHeader.biWidth;
                int height = Math.Abs(bmi.bmiHeader.biHeight);
                if (width <= 0 || height <= 0)
                    return null;

                bmi.bmiHeader.biPlanes = 1;
                bmi.bmiHeader.biBitCount = 32;
                bmi.bmiHeader.biCompression = BI_RGB;
                bmi.bmiHeader.biHeight = -height;

                int stride = width * 4;
                int imageSize = stride * height;

                IntPtr buffer = Marshal.AllocHGlobal(imageSize);
                try
                {
                    if (GetDIBits(hdc, hbmColor, 0, (uint)height, buffer, ref bmi, DIB_RGB_COLORS) == 0)
                        return null;

                    var pixelSize = new PixelSize(width, height);
                    var dpi = new Vector(96, 96);
                    var bmp = new WriteableBitmap(PixelFormat.Bgra8888, AlphaFormat.Premul, buffer, pixelSize, dpi, stride);

                    return bmp;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }
        finally
        {
            if (hbmColor != IntPtr.Zero)
                DeleteObject(hbmColor);
            if (hbmMask != IntPtr.Zero)
                DeleteObject(hbmMask);
        }
    }
    #endregion

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_LARGEICON = 0x000000000;
    private const uint SHGFI_SMALLICON = 0x000000001;
    private const uint DIB_RGB_COLORS = 0;
    private const uint BI_RGB = 0;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ICONINFO
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        public uint bmiColors;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, IntPtr lpvBits, ref BITMAPINFO lpbi, uint uUsage);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);
}
