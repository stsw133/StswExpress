using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Runtime.InteropServices;

namespace StswExpress.Avalonia;
/// <summary>
/// Collection of extension methods for various types and objects. These methods simplify common tasks and provide additional functionality beyond what is available in the standard Avalonia API.
/// </summary>
public static partial class StswExtensions
{
    #region Convert extensions
    /// <summary>
    /// Converts an <see cref="Bitmap"/> to a byte array using PNG encoding.
    /// </summary>
    /// <param name="value">The bitmap to convert.</param>
    /// <returns>A byte array representing the encoded image.</returns>
    public static byte[] ToBytes(this Bitmap value)
    {
        ArgumentNullException.ThrowIfNull(value);

        using var memoryStream = new MemoryStream();
        value.Save(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Converts a <see cref="Geometry"/> to an Avalonia <see cref="Bitmap"/>.
    /// </summary>
    /// <param name="geometry">The geometry to convert.</param>
    /// <param name="size">Height and width of the output image.</param>
    /// <param name="fill">Fill brush of the output image.</param>
    /// <param name="stroke">Stroke brush of the output image.</param>
    /// <param name="strokeThickness">Stroke thickness of the output image.</param>
    /// <returns>The converted Avalonia <see cref="Bitmap"/>.</returns>
    public static Bitmap ToAvaloniaBitmap(this Geometry geometry, double size, IBrush? fill = null, IBrush? stroke = null, double strokeThickness = 0)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var renderTarget = new RenderTargetBitmap(new PixelSize((int)size, (int)size));

        using (var context = renderTarget.CreateDrawingContext())
        {
            var pen = stroke != null ? new Pen(stroke, strokeThickness) : null;
            context.DrawGeometry(fill, pen, geometry);
        }

        return renderTarget;
    }
    #endregion

    #region Color extensions
    /// <summary>
    /// Calculates the brightness of a <see cref="Color"/> using the HSL color model.
    /// </summary>
    /// <param name="c">The color to calculate brightness for.</param>
    /// <returns>The brightness value ranging from 0 (darkest) to 1 (brightest).</returns>
    public static float GetBrightness(this Color c)
    {
        var r = c.R / 255f;
        var g = c.G / 255f;
        var b = c.B / 255f;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));

        return (max + min) / 2f;
    }

    /// <summary>
    /// Converts a <see cref="Color"/> to a hexadecimal color string.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The hexadecimal color string representation of the color (e.g., "#RRGGBB" or "#AARRGGBB").</returns>
    public static string ToHex(this Color color)
    {
        if (color.A < 255)
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        else
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Converts a <see cref="Color"/> to an integer representation (ARGB).
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The integer representation of the color in ARGB format.</returns>
    public static int ToInt(this Color color) => (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;

    /// <summary>
    /// Converts an integer representation of a color (ARGB) to a <see cref="Color"/>.
    /// </summary>
    /// <param name="argb">The integer representation of the color in ARGB format.</param>
    /// <returns>The converted <see cref="Color"/>.</returns>
    public static Color ToMediaColor(this int argb)
        => Color.FromArgb(
            (byte)((argb >> 24) & 0xFF),
            (byte)((argb >> 16) & 0xFF),
            (byte)((argb >> 8) & 0xFF),
            (byte)(argb & 0xFF));
    #endregion
}