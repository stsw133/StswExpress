using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

/// <summary>
/// Collection of extension methods for various types and objects. These methods simplify common tasks and provide additional functionality beyond what is available in the standard WPF API.
/// </summary>
public static class StswExtensions
{
    #region Bool extensions
    /// <summary>
    /// Returns <c>true</c> if a value is between two other values (inclusive), <c>false</c> otherwise.
    /// </summary>
    public static bool Between<T>(this T value, T start, T end) => Comparer<T>.Default.Compare(value, start) >= 0 && Comparer<T>.Default.Compare(value, end) <= 0;

    /// <summary>
    /// Returns <c>true</c> if a value is contained in a collection, <c>false</c> otherwise.
    /// </summary>
    public static bool In<T>(this T value, IEnumerable<T> input) => input.Any(n => Equals(n, value));

    /// <summary>
    /// Returns <c>true</c> if a value is contained in a collection, <c>false</c> otherwise.
    /// </summary>
    public static bool In<T>(this T value, params T[] input) => input.Any(n => Equals(n, value));
    #endregion

    #region Byte[] extensions
    /// <summary>
    /// Converts <see cref="ImageSource"/> to <see cref="byte"/>[].
    /// </summary>
    public static byte[] ToBytes(this ImageSource value)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(value as BitmapSource));
        using (var memoryStream = new MemoryStream())
        {
            encoder.Save(memoryStream);
            return memoryStream.ToArray();
        }
    }

    /// <summary>
    /// Converts <see cref="SecureString"/> to <see cref="byte"/>[].
    /// </summary>
    public static byte[] ToBytes(this SecureString value) => Encoding.UTF8.GetBytes(new NetworkCredential(string.Empty, value).Password);
    #endregion

    #region Collection extensions
    /// <summary>
    /// Converts <see cref="DataTable"/> to <see cref="List{T}"/>.
    /// </summary>
    public static List<T> ToObjectList<T>(this DataTable value) where T : class, new()
    {
        var result = new List<T>();

        foreach (var row in value.AsEnumerable())
        {
            var obj = new T();

            foreach (var prop in obj.GetType().GetProperties().Where(p => p.Name.ToLower().In(value.Columns.Cast<DataColumn>().Select(x => x.ColumnName.ToLower()))))
            {
                try
                {
                    var propertyInfo = obj.GetType().GetProperty(prop.Name);

                    if (propertyInfo?.PropertyType == typeof(ImageSource))
                        propertyInfo.SetValue(obj, ((byte[])row[prop.Name]).ToBitmapImage(), null);
                    else if (propertyInfo?.PropertyType != typeof(object))
                        propertyInfo?.SetValue(obj, row[prop.Name].ConvertTo(propertyInfo.PropertyType), null);
                    else
                        propertyInfo?.SetValue(obj, row[prop.Name], null);
                }
                catch
                {
                    continue;
                }
            }

            result.Add(obj);
        }

        return result;
    }

    /// <summary>
    /// Converts <see cref="IEnumerable{T}"/> to <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> value) => new ObservableCollection<T>(value);

    /// <summary>
    /// Converts <see cref="IEnumerable{T}"/> to <see cref="StswCollection{T}"/>.
    /// </summary>
    public static StswCollection<T> ToStswCollection<T>(this IEnumerable<T> value) where T : StswCollectionItem => new StswCollection<T>(value);

    /// <summary>
    /// Converts <see cref="IDictionary{TKey, TValue}"/> to <see cref="StswDictionary{TKey, TValue}"/>.
    /// </summary>
    public static StswDictionary<T1, T2> ToStswDictionary<T1, T2>(this IDictionary<T1, T2> value) => new StswDictionary<T1, T2>(value);
    #endregion

    #region Color extensions
    /// <summary>
    /// Makes color from HSL values.
    /// </summary>
    /// <param name="hue">Between 0 and 360.</param>
    /// <param name="saturation">Between 0 and 100.</param>
    /// <param name="lightness">Between 0 and 100.</param>
    /// <returns></returns>
    public static Color FromHsl(int hue, int saturation, int lightness)
    {
        double h = hue / 360.0;
        double s = saturation / 100.0;
        double l = lightness / 100.0;

        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h * 6) % 2 - 1));
        double m = l - c / 2;

        double r, g, b;
        if (h < 1.0 / 6)
        {
            r = c;
            g = x;
            b = 0;
        }
        else if (h < 2.0 / 6)
        {
            r = x;
            g = c;
            b = 0;
        }
        else if (h < 3.0 / 6)
        {
            r = 0;
            g = c;
            b = x;
        }
        else if (h < 4.0 / 6)
        {
            r = 0;
            g = x;
            b = c;
        }
        else if (h < 5.0 / 6)
        {
            r = x;
            g = 0;
            b = c;
        }
        else
        {
            r = c;
            g = 0;
            b = x;
        }

        byte red = (byte)((r + m) * 255);
        byte green = (byte)((g + m) * 255);
        byte blue = (byte)((b + m) * 255);

        return Color.FromRgb(red, green, blue);
    }

    /// <summary>
    /// Gets HSL values from color.
    /// </summary>
    public static void GetHsl(this Color color, out double hue, out double saturation, out double lightness)
    {
        double r = (double)color.R / 255.0;
        double g = (double)color.G / 255.0;
        double b = (double)color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        /// calculate hue
        if (delta == 0)
            hue = 0;
        else if (max == r)
            hue = ((g - b) / delta) % 6;
        else if (max == g)
            hue = ((b - r) / delta) + 2;
        else
            hue = ((r - g) / delta) + 4;

        hue = Math.Round(hue * 60);

        /// make sure hue is positive
        if (hue < 0)
            hue += 360;

        /// calculate lightness
        lightness = (max + min) / 2;

        /// calculate saturation
        if (delta == 0)
            saturation = 0;
        else
            saturation = delta / (1 - Math.Abs(2 * lightness - 1));

        hue = Math.Round(hue);
        saturation = Math.Round(saturation * 100);
        lightness = Math.Round(lightness * 100);
    }

    /// <summary>
    /// Converts <see cref="byte"/>[] to <see cref="BitmapImage"/>.
    /// </summary>
    public static BitmapImage? ToBitmapImage(this byte[] value)
    {
        if (!value.Any())
            return null;

        var result = new BitmapImage();
        using (var mem = new MemoryStream(value))
        {
            mem.Position = 0;
            result.BeginInit();
            result.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.UriSource = null;
            result.StreamSource = mem;
            result.EndInit();
        }
        result.Freeze();

        return result;
    }

    /// <summary>
    /// Converts <see cref="SolidColorBrush"/> to <see cref="System.Drawing.Color"/>.
    /// </summary>
    public static Color ToColor(this SolidColorBrush value) => Color.FromArgb(value.Color.A, value.Color.R, value.Color.G, value.Color.B);
    #endregion

    #region Finding extensions
    /// <summary>
    /// Finds the first visual ancestor of a specific type for the given control.
    /// </summary>
    public static T? FindVisualAncestor<T>(DependencyObject obj) where T : DependencyObject
    {
        if (obj != null)
        {
            var dependObj = obj;
            do
            {
                dependObj = GetParent(dependObj);
                if (dependObj is T)
                    return dependObj as T;
            }
            while (dependObj != null);
        }

        return null;
    }

    /// <summary>
    /// Finds the first visual child of a specific type for the given control.
    /// </summary>
    public static T? FindVisualChild<T>(DependencyObject obj) where T : Visual
    {
        T? child = default;

        var numVisuals = VisualTreeHelper.GetChildrenCount(obj);
        for (int i = 0; i < numVisuals; i++)
        {
            var v = (Visual)VisualTreeHelper.GetChild(obj, i);
            child = v as T;

            child ??= FindVisualChild<T>(v);

            if (child != null)
                break;
        }

        return child;
    }

    /// <summary>
    /// Finds all visual children of a specific type for the given control.
    /// </summary>
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
    {
        if (obj != null)
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                if (VisualTreeHelper.GetChild(obj, i) is DependencyObject child and not null)
                {
                    if (child is T t)
                        yield return t;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
    }

    /// <summary>
    /// Gets the parent of the given control.
    /// </summary>
    public static DependencyObject? GetParent(DependencyObject obj)
    {
        if (obj == null)
            return null;
        if (obj is ContentElement)
        {
            var parent = ContentOperations.GetParent(obj as ContentElement);
            if (parent != null)
                return parent;
            if (obj is FrameworkContentElement)
                return (obj as FrameworkContentElement)?.Parent;
            return null;
        }

        return VisualTreeHelper.GetParent(obj);
    }
    #endregion

    #region Function extensions
    /// <summary>
    /// Tries to execute an action multiple times with a specified interval between each try, until it succeeds or reaches a maximum number of tries.
    /// </summary>
    public static void TryMultipleTimes(this Action action, int maxTries = 5, int msInterval = 200)
    {
        while (maxTries > 0)
        {
            try
            {
                action.Invoke();
                break;
            }
            catch
            {
                if (--maxTries <= 0)
                    throw;

                Thread.Sleep(msInterval);
            }
        }
    }

    /// <summary>
    /// Tries to execute a function multiple times with a specified interval between each try, until it succeeds or reaches a maximum number of tries.
    /// </summary>
    public static T? TryMultipleTimes<T>(this Func<T> func, int maxTries = 5, int msInterval = 200)
    {
        while (maxTries > 0)
        {
            try
            {
                return func();
            }
            catch
            {
                if (--maxTries <= 0)
                    throw;

                Thread.Sleep(msInterval);
            }
        }
        return default;
    }
    #endregion

    #region Text extensions
    /// <summary>
    /// Capitalizes the first letter of a string and makes the rest lowercase.
    /// </summary>
    public static string Capitalize(this string text) => char.ToUpper(text.First()) + text[1..].ToLower();

    /// <summary>
    /// Converts <see cref="Color"/> to <see cref="string"/>.
    /// </summary>
    public static string ToHtml(this Color color) => new ColorConverter().ConvertToString(color) ?? string.Empty;

    /// <summary>
    /// Converts <see cref="string"/> to <see cref="SecureString"/>.
    /// </summary>
    public static SecureString ToSecureString(this string value) => new NetworkCredential(string.Empty, value).SecurePassword;

    /// <summary>
    /// Trims a string of a specified substring at the end.
    /// </summary>
    public static string TrimEnd(this string source, string value) => !source.EndsWith(value) ? source : source.Remove(source.LastIndexOf(value));

    /// <summary>
    /// Trims a string of a specified substring at the start.
    /// </summary>
    public static string TrimStart(this string source, string value) => !source.StartsWith(value) ? source : source[value.Length..];
    #endregion

    #region Universal extensions
    /// <summary>
    /// Converts <see cref="object"/> to different type.
    /// </summary>
    /// <param name="o">Object to convert.</param>
    /// <param name="t">Type to convert to.</param>
    /// <returns>Object of different type.</returns>
    public static object? ConvertTo(this object o, Type t)
    {
        if (o == null || o == DBNull.Value)
            return Nullable.GetUnderlyingType(t) == null ? default : Convert.ChangeType(null, t);
        else
        {
            var underlyingType = Nullable.GetUnderlyingType(t);
            return underlyingType == null
                ? Convert.ChangeType(o, t, CultureInfo.InvariantCulture)
                : Convert.ChangeType(o, underlyingType, CultureInfo.InvariantCulture);
        }
    }
    #endregion
}
