﻿using System;
using System.Collections;
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
    /// Clones an <see cref="IList"/> into another <see cref="IList"/> while preserving the items in the new list.
    /// </summary>
    public static IList Clone(this IList source)
    {
        if (Activator.CreateInstance(source.GetType()) is IList clonedList)
        {
            foreach (var item in source)
            {
                if (item is ICloneable cloneableItem)
                    clonedList.Add(cloneableItem.Clone());
                else
                    clonedList.Add(item);
            }
            return clonedList;
        }
        else throw new ArgumentNullException("The source is not a proper IList.");
    }

    /// <summary>
    /// Converts <see cref="DataTable"/> to <see cref="List{T}"/>.
    /// </summary>
    public static List<T> ToObjectList<T>(this DataTable dt) where T : class, new()
    {
        var result = new List<T>();
        var objProps = new T().GetType().GetProperties().ToList();

        var indexer = new List<int>();
        foreach (DataColumn col in dt.Columns)
            indexer.Add(objProps.FindIndex(x => x.Name.ToLower() == col.ColumnName.ToLower()));
        int[] mappings = indexer.Where(x => x >= 0).ToArray();

        foreach (var row in dt.AsEnumerable())
        {
            var obj = new T();

            for (int i = 0; i < mappings.Length; i++)
            {
                try
                {
                    var propertyInfo = objProps[mappings[i]];

                    if (propertyInfo?.PropertyType != typeof(object))
                        propertyInfo?.SetValue(obj, row[i].ConvertTo(propertyInfo.PropertyType), null);
                    else
                        propertyInfo?.SetValue(obj, row[i], null);
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
    public static StswCollection<T> ToStswCollection<T>(this IEnumerable<T> value) where T : IStswCollectionItem => new StswCollection<T>(value);

    /// <summary>
    /// Converts <see cref="IDictionary{TKey, TValue}"/> to <see cref="StswDictionary{TKey, TValue}"/>.
    /// </summary>
    public static StswDictionary<T1, T2> ToStswDictionary<T1, T2>(this IDictionary<T1, T2> value) => new StswDictionary<T1, T2>(value);
    #endregion

    #region Color extensions
    /// <summary>
    /// Makes color from HSL values.
    /// </summary>
    /// <param name="alpha">Between 0 and 255.</param>
    /// <param name="hue">Between 0 and 360.</param>
    /// <param name="saturation">Between 0 and 1.</param>
    /// <param name="lightness">Between 0 and 1.</param>
    /// <returns></returns>
    public static Color FromAhsl(byte alpha, double hue, double saturation, double lightness)
    {
        double h = hue / 360.0;
        double c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        double x = c * (1 - Math.Abs((h * 6) % 2 - 1));
        double m = lightness - c / 2;

        if (h < 1.0 / 6)
            return Color.FromArgb(alpha, (byte)Math.Round((c + m) * 255), (byte)Math.Round((x + m) * 255), (byte)Math.Round(m * 255));
        else if (h < 2.0 / 6)
            return Color.FromArgb(alpha, (byte)Math.Round((x + m) * 255), (byte)Math.Round((c + m) * 255), (byte)Math.Round(m * 255));
        else if (h < 3.0 / 6)
            return Color.FromArgb(alpha, (byte)Math.Round(m * 255), (byte)Math.Round((c + m) * 255), (byte)Math.Round((x + m) * 255));
        else if (h < 4.0 / 6)
            return Color.FromArgb(alpha, (byte)Math.Round(m * 255), (byte)Math.Round((x + m) * 255), (byte)Math.Round((c + m) * 255));
        else if (h < 5.0 / 6)
            return Color.FromArgb(alpha, (byte)Math.Round((x + m) * 255), (byte)Math.Round(m * 255), (byte)Math.Round((c + m) * 255));
        else
            return Color.FromArgb(alpha, (byte)Math.Round((c + m) * 255), (byte)Math.Round(m * 255), (byte)Math.Round((x + m) * 255));
    }

    /// <summary>
    /// Makes color from HSL values.
    /// </summary>
    /// <param name="hue">Between 0 and 360.</param>
    /// <param name="saturation">Between 0 and 1.</param>
    /// <param name="lightness">Between 0 and 1.</param>
    /// <returns></returns>
    public static Color FromHsl(double hue, double saturation, double lightness) => FromAhsl(255, hue, saturation, lightness);

    /// <summary>
    /// Gets HSL values from color.
    /// </summary>
    public static void ToHsl(this Color color, out double hue, out double saturation, out double lightness)
    {
        var drawingColor = color.ToDrawingColor();

        hue = drawingColor.GetHue();
        saturation = drawingColor.GetSaturation();
        lightness = drawingColor.GetBrightness();
    }

    /// <summary>
    /// Makes color from HSV values.
    /// </summary>
    /// <param name="alpha">Between 0 and 255.</param>
    /// <param name="hue">Between 0 and 360.</param>
    /// <param name="saturation">Between 0 and 1.</param>
    /// <param name="value">Between 0 and 1.</param>
    /// <returns></returns>
    public static Color FromAhsv(byte alpha, double hue, double saturation, double value)
    {
        int h = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        byte v = Convert.ToByte(value);
        byte p = Convert.ToByte(value * (1 - saturation));
        byte q = Convert.ToByte(value * (1 - f * saturation));
        byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

        if (h == 0)
            return Color.FromArgb(alpha, v, t, p);
        else if (h == 1)
            return Color.FromArgb(alpha, q, v, p);
        else if (h == 2)
            return Color.FromArgb(alpha, p, v, t);
        else if (h == 3)
            return Color.FromArgb(alpha, p, q, v);
        else if (h == 4)
            return Color.FromArgb(alpha, t, p, v);
        else
            return Color.FromArgb(alpha, v, p, q);
    }

    /// <summary>
    /// Makes color from HSV values.
    /// </summary>
    /// <param name="hue">Between 0 and 360.</param>
    /// <param name="saturation">Between 0 and 1.</param>
    /// <param name="value">Between 0 and 1.</param>
    /// <returns></returns>
    public static Color FromHsv(double hue, double saturation, double value) => FromAhsv(255, hue, saturation, value);

    /// <summary>
    /// Gets HSV values from color.
    /// </summary>
    public static void ToHsv(this Color color, out double hue, out double saturation, out double value)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        hue = color.ToDrawingColor().GetHue();
        saturation = (max == 0) ? 0 : 1d - (1d * min / max);
        value = max / 255d;
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

    /// <summary>
    /// Converts <see cref="Color"/> to <see cref="System.Drawing.Color"/>.
    /// </summary>
    public static System.Drawing.Color ToDrawingColor(this Color value) => System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);

    /// <summary>
    /// Converts <see cref="System.Drawing.Color"/> to <see cref="Color"/>.
    /// </summary>
    public static Color ToMediaColor(this System.Drawing.Color value) => Color.FromArgb(value.A, value.R, value.G, value.B);

    /// <summary>
    /// Converts <see cref="Color"/> to <see cref="string"/>.
    /// </summary>
    public static string ToHtml(this Color color) => new ColorConverter().ConvertToString(color) ?? string.Empty;
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
    /// Returns a new string of a specified length in which the beginning of the current string is padded with a specified text.
    /// </summary>
    public static string? PadLeft(this string text, int totalWidth, string paddingString)
    {
        if (paddingString == null)
            throw new ArgumentNullException(nameof(paddingString));

        if (totalWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(totalWidth), "The total width cannot be negative.");

        if (text == null || text.Length >= totalWidth)
            return text;

        int paddingCount = (totalWidth - text.Length + paddingString.Length - 1) / paddingString.Length;
        return paddingString[..paddingCount] + text;
    }

    /// <summary>
    /// Returns a new string of a specified length in which the end of the current string is padded with a specified text.
    /// </summary>
    public static string? PadRight(this string text, int totalWidth, string paddingString)
    {
        if (paddingString == null)
            throw new ArgumentNullException(nameof(paddingString));

        if (totalWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(totalWidth), "The total width cannot be negative.");

        if (text == null || text.Length >= totalWidth)
            return text;

        int paddingCount = (totalWidth - text.Length + paddingString.Length - 1) / paddingString.Length;
        return text + paddingString[..paddingCount];
    }

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
    /// Converts <see cref="T"/> to different type.
    /// </summary>
    /// <param name="o">Object to convert.</param>
    /// <returns>Object of different type.</returns>
    public static T? ConvertTo<T>(this object o)
    {
        if (o == null || o == DBNull.Value)
            return Nullable.GetUnderlyingType(typeof(T?)) == null ? default : (T?)(object?)null;
        else
        {
            var underlyingType = Nullable.GetUnderlyingType(typeof(T));
            return underlyingType == null
                ? (T)Convert.ChangeType(o, typeof(T), CultureInfo.InvariantCulture)
                : (T)Convert.ChangeType(o, underlyingType, CultureInfo.InvariantCulture);
        }
    }

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
