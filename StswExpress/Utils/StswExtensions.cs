using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;
/// <summary>
/// Collection of extension methods for various types and objects. These methods simplify common tasks and provide additional functionality beyond what is available in the standard WPF API.
/// </summary>
public static partial class StswExtensions
{
    #region Clone extensions
    /// <summary>
    /// Clones a <see cref="BindingBase"/> object, creating a deep copy of its settings.
    /// Supports <see cref="Binding"/>, <see cref="MultiBinding"/>, and <see cref="PriorityBinding"/>.
    /// Does not support cyclic references.
    /// </summary>
    /// <param name="bindingBase">The <see cref="BindingBase"/> to clone.</param>
    /// <returns>A cloned <see cref="BindingBase"/> object.</returns>
    public static BindingBase Clone(this BindingBase bindingBase)
    {
        ArgumentNullException.ThrowIfNull(bindingBase);

        switch (bindingBase)
        {
            case Binding binding:
                {
                    var result = new Binding
                    {
                        AsyncState = binding.AsyncState,
                        BindingGroupName = binding.BindingGroupName,
                        BindsDirectlyToSource = binding.BindsDirectlyToSource,
                        Converter = binding.Converter,
                        ConverterCulture = binding.ConverterCulture,
                        ConverterParameter = binding.ConverterParameter,
                        FallbackValue = binding.FallbackValue,
                        IsAsync = binding.IsAsync,
                        Mode = binding.Mode,
                        NotifyOnSourceUpdated = binding.NotifyOnSourceUpdated,
                        NotifyOnTargetUpdated = binding.NotifyOnTargetUpdated,
                        NotifyOnValidationError = binding.NotifyOnValidationError,
                        Path = binding.Path,
                        StringFormat = binding.StringFormat,
                        TargetNullValue = binding.TargetNullValue,
                        UpdateSourceExceptionFilter = binding.UpdateSourceExceptionFilter,
                        UpdateSourceTrigger = binding.UpdateSourceTrigger,
                        ValidatesOnDataErrors = binding.ValidatesOnDataErrors,
                        ValidatesOnExceptions = binding.ValidatesOnExceptions,
                        XPath = binding.XPath,
                    };

                    if (binding.ElementName != null)
                        result.ElementName = binding.ElementName;
                    else if (binding.RelativeSource != null)
                        result.RelativeSource = binding.RelativeSource;
                    else if (binding.Source != null)
                        result.Source = binding.Source;

                    foreach (var validationRule in binding.ValidationRules)
                        result.ValidationRules.Add(validationRule);

                    return result;
                }

            case MultiBinding multiBinding:
                {
                    var result = new MultiBinding
                    {
                        BindingGroupName = multiBinding.BindingGroupName,
                        Converter = multiBinding.Converter,
                        ConverterCulture = multiBinding.ConverterCulture,
                        ConverterParameter = multiBinding.ConverterParameter,
                        FallbackValue = multiBinding.FallbackValue,
                        Mode = multiBinding.Mode,
                        NotifyOnSourceUpdated = multiBinding.NotifyOnSourceUpdated,
                        NotifyOnTargetUpdated = multiBinding.NotifyOnTargetUpdated,
                        NotifyOnValidationError = multiBinding.NotifyOnValidationError,
                        StringFormat = multiBinding.StringFormat,
                        TargetNullValue = multiBinding.TargetNullValue,
                        UpdateSourceExceptionFilter = multiBinding.UpdateSourceExceptionFilter,
                        UpdateSourceTrigger = multiBinding.UpdateSourceTrigger,
                        ValidatesOnDataErrors = multiBinding.ValidatesOnDataErrors,
                        ValidatesOnExceptions = multiBinding.ValidatesOnExceptions,
                    };

                    foreach (var validationRule in multiBinding.ValidationRules)
                        result.ValidationRules.Add(validationRule);

                    foreach (var childBinding in multiBinding.Bindings)
                        result.Bindings.Add(childBinding.Clone());

                    return result;
                }

            case PriorityBinding priorityBinding:
                {
                    var result = new PriorityBinding
                    {
                        BindingGroupName = priorityBinding.BindingGroupName,
                        FallbackValue = priorityBinding.FallbackValue,
                        StringFormat = priorityBinding.StringFormat,
                        TargetNullValue = priorityBinding.TargetNullValue,
                    };

                    foreach (var childBinding in priorityBinding.Bindings)
                        result.Bindings.Add(childBinding.Clone());

                    return result;
                }

            default:
                throw new NotSupportedException("Failed to clone binding");
        }
    }

    /// <summary>
    /// Attempts to create a shallow copy of each item in an <see cref="IEnumerable"/>.
    /// Items implementing <see cref="ICloneable"/> are cloned, others are returned as-is.
    /// This does not guarantee deep copying of complex objects.
    /// </summary>
    /// <param name="source">The source enumerable to clone.</param>
    /// <returns>A new <see cref="IEnumerable"/> containing cloned items when possible; if an item does not implement <see cref="ICloneable"/>, the original item is returned. This method ensures that the original collection remains unmodified.</returns>
    /// <remarks>
    /// This method does not perform a deep clone of items unless they explicitly implement <see cref="ICloneable"/>. Items that are not cloneable are included directly in the new collection, which may affect mutability depending on the item's type.
    /// </remarks>
    public static IEnumerable TryClone(this IEnumerable source)
    {
        ArgumentNullException.ThrowIfNull(source);

        foreach (var item in source)
        {
            if (item is ICloneable cloneableItem)
                yield return cloneableItem.Clone();
            else
                yield return item;
        }
    }
    #endregion

    #region Convert extensions
    /// <summary>
    /// Converts an <see cref="ImageSource"/> to a byte array using the specified <see cref="BitmapEncoder"/>.
    /// </summary>
    /// <param name="value">The <see cref="ImageSource"/> to convert.</param>
    /// <param name="encoder">The <see cref="BitmapEncoder"/> to use for encoding the image. If not specified, <see cref="PngBitmapEncoder"/> will be used by default.</param>
    /// <returns>A byte array representing the encoded image.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is not a <see cref="BitmapSource"/>.</exception>
    public static byte[] ToBytes(this ImageSource value, BitmapEncoder? encoder = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value is not BitmapSource bitmapSource)
            throw new ArgumentException($"Value must be a {nameof(BitmapSource)}.", nameof(value));

        encoder ??= new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

        using var memoryStream = new MemoryStream();
        encoder.Save(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Converts a <see cref="System.Drawing.Bitmap"/> to an <see cref="ImageSource"/>.
    /// Frees the underlying HBitmap handle after conversion.
    /// Throws an exception if the bitmap is invalid.
    /// </summary>
    /// <param name="bmp">The bitmap to convert.</param>
    /// <returns>The converted <see cref="ImageSource"/>.</returns>
    public static ImageSource ToImageSource(this System.Drawing.Bitmap bmp)
    {
        IntPtr handle = bmp.GetHbitmap();
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                handle,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            if (handle != IntPtr.Zero)
                DeleteObject(handle);
        }
    }

    /// <summary>
    /// Converts an <see cref="Icon"/> to an <see cref="ImageSource"/>.
    /// Frees the underlying HBitmap handle after conversion.
    /// Throws an exception if the bitmap is invalid.
    /// </summary>
    /// <param name="icon">The <see cref="Icon"/> to convert.</param>
    /// <returns>The converted <see cref="ImageSource"/>.</returns>
    public static ImageSource ToImageSource(this System.Drawing.Icon icon)
    {
        using var bitmap = icon.ToBitmap();
        var hBitmap = bitmap.GetHbitmap();
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            if (hBitmap != IntPtr.Zero)
                DeleteObject(hBitmap);
        }
    }

    /// <summary>
    /// Converts a <see cref="Geometry"/> to an <see cref="ImageSource"/>.
    /// </summary>
    /// <param name="geometry">The geometry to convert.</param>
    /// <param name="size">Height and width of the output image.</param>
    /// <param name="fill">Fill brush of the output image.</param>
    /// <param name="stroke">Stroke brush of the output image.</param>
    /// <param name="strokeThickness">Stroke thickness of the output image.</param>
    /// <param name="dpi">DPI of the output image. Defaults to 96.</param>
    /// <returns>The converted <see cref="ImageSource"/>.</returns>
    public static ImageSource ToImageSource(this Geometry geometry, double size, Brush? fill = null, Brush? stroke = null, double strokeThickness = 0, double dpi = 96)
    {
        var drawingVisual = new DrawingVisual();
        var pen = stroke != null ? new Pen(stroke, strokeThickness) : null;

        using (var drawingContext = drawingVisual.RenderOpen())
            drawingContext.DrawGeometry(fill, pen, geometry);

        var renderTargetBitmap = new RenderTargetBitmap(
            (int)size,
            (int)size,
            dpi,
            dpi,
            PixelFormats.Pbgra32);

        renderTargetBitmap.Render(drawingVisual);

        return renderTargetBitmap;
    }
    #endregion

    #region Color extensions
    /// <summary>
    /// Converts a <see cref="Color"/> to a <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="value">The <see cref="Color"/> to convert.</param>
    /// <returns>The converted <see cref="System.Drawing.Color"/>.</returns>
    public static System.Drawing.Color ToDrawingColor(this Color value) => System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);

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

    /// <summary>
    /// Converts a <see cref="System.Drawing.Color"/> to a <see cref="Color"/>.
    /// </summary>
    /// <param name="value">The <see cref="System.Drawing.Color"/> to convert.</param>
    /// <returns>The converted <see cref="Color"/>.</returns>
    public static Color ToMediaColor(this System.Drawing.Color value) => Color.FromArgb(value.A, value.R, value.G, value.B);
    #endregion

    #region Process extensions
    /// <summary>
    /// Determines the user that owns the specified process.
    /// </summary>
    /// <param name="process">The process whose owner is to be determined.</param>
    /// <returns>The username of the owner of the process, or <see langword="null"/> if it cannot be determined.</returns>
    public static string? GetUser(this Process process)
    {
        var processHandle = IntPtr.Zero;
        try
        {
            OpenProcessToken(process.Handle, 8, out processHandle);
            using var identity = new WindowsIdentity(processHandle);
            var user = identity.Name;
            return user.Contains('\\') ? user[(user.IndexOf('\\') + 1)..] : user;
        }
        catch
        {
            return null;
        }
        finally
        {
            if (processHandle != IntPtr.Zero)
                CloseHandle(processHandle);
        }
    }
    #endregion

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr hObject);

    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]

    private static extern bool DeleteObject([In] IntPtr hObject);
}
