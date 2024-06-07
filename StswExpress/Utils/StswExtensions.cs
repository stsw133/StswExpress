using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
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

    /// <summary>
    /// Returns <c>true</c> if a <see cref="Type"/> is numeric, <c>false</c> otherwise.
    /// </summary>
    public static bool IsNumericType(this Type type)
    {
        return type.In(typeof(sbyte), typeof(sbyte?),
                       typeof(byte), typeof(byte?),
                       typeof(short), typeof(short?),
                       typeof(ushort), typeof(ushort?),
                       typeof(int), typeof(int?),
                       typeof(uint), typeof(uint?),
                       typeof(long), typeof(long?),
                       typeof(float), typeof(float?),
                       typeof(double), typeof(double?),
                       typeof(decimal), typeof(decimal?),
                       typeof(nint), typeof(nint?),
                       typeof(nuint), typeof(nuint?));
    }
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

    #region Clone extensions
    /// <summary>
    /// Clones <see cref="BindingBase"/>.
    /// </summary>
    public static BindingBase Clone(this BindingBase bindingBase)
    {
        if (bindingBase is Binding binding)
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
            else if(binding.Source != null)
                result.Source = binding.Source;

            foreach (var validationRule in binding.ValidationRules)
                result.ValidationRules.Add(validationRule);

            return result;
        }

        if (bindingBase is MultiBinding multiBinding)
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
                ValidatesOnExceptions = multiBinding.ValidatesOnDataErrors,
            };

            foreach (var validationRule in multiBinding.ValidationRules)
                result.ValidationRules.Add(validationRule);

            foreach (var childBinding in multiBinding.Bindings)
                result.Bindings.Add(childBinding.Clone());

            return result;
        }

        if (bindingBase is PriorityBinding priorityBinding)
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

        throw new NotSupportedException("Failed to clone binding");
    }

    /// <summary>
    /// Clones an <see cref="IEnumerable"/> into another <see cref="IEnumerable"/> while preserving the items in the new list.
    /// </summary>
    [Obsolete($"You can use '{nameof(Copy)}' instead.")]
    public static IEnumerable Clone(this IEnumerable source)
    {
        foreach (var item in source)
        {
            if (item is ICloneable cloneableItem)
                yield return cloneableItem.Clone();
            else
                yield return item;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static object? Copy(this object o)
    {
        var type = o.GetType();
        var target = Activator.CreateInstance(type);

        foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (!pi.CanWrite)
                continue;

            if (IsListType(pi.PropertyType, out var innerType))
            {
                var listType = typeof(List<>).MakeGenericType(innerType!);
                var list = (IList?)Activator.CreateInstance(listType);
                if (pi.GetValue(o, null) is IList oldList)
                    foreach (var item in oldList)
                        list?.Add(item.Copy());
                pi.SetValue(target, list);
            }
            else if (pi.PropertyType.IsValueType || pi.PropertyType.IsEnum || pi.PropertyType == typeof(string))
                pi.SetValue(target, pi.GetValue(o, null), null);
            else
            {
                var propValue = pi.GetValue(o, null);
                pi.SetValue(target, propValue == null ? null : Copy(propValue), null);
            }
        }

        return target;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="innerType"></param>
    /// <returns></returns>
    internal static bool IsListType(this Type type, out Type? innerType)
    {
        var interfaceTest = new Func<Type, Type?>(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>) ? i.GetGenericArguments().Single() : null);

        innerType = interfaceTest(type);
        if (innerType != null)
            return true;

        foreach (var i in type.GetInterfaces())
        {
            innerType = interfaceTest(i);
            if (innerType != null)
                return true;
        }

        return false;
    }
    #endregion

    #region Convert extensions
    /// <summary>
    /// Converts <see cref="{T}"/> to different type.
    /// </summary>
    /// <param name="o">Object to convert.</param>
    /// <returns>Object of different type.</returns>
    public static T? ConvertTo<T>(this object o)
    {
        if (o == null || o == DBNull.Value)
            return Nullable.GetUnderlyingType(typeof(T?)) == null ? default : (T?)(object?)null;
        else if (typeof(T).IsEnum)
        {
            if (Enum.GetUnderlyingType(typeof(T)) == o.GetType())
                return (T)Enum.ToObject(typeof(T), o);
            if (Enum.TryParse(typeof(T), o.ToString(), out object? result))
                return (T?)result;
            return default;
        }
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
    public static object? ConvertTo(this object? o, Type t)
    {
        var underlyingType = Nullable.GetUnderlyingType(t);

        if (o == null || o == DBNull.Value)
            return underlyingType == null ? default : Convert.ChangeType(null, t);
        else if (t.IsEnum || underlyingType?.IsEnum == true)
            return underlyingType == null
                ? (Enum.TryParse(t, o.ToString(), out var result1) ? result1 : null)
                : (Enum.TryParse(underlyingType, o.ToString(), out var result2) ? result2 : null);
        else
            return underlyingType == null
                ? Convert.ChangeType(o, t, CultureInfo.InvariantCulture)
                : Convert.ChangeType(o, underlyingType, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts <see cref="DataTable"/> to <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static IEnumerable<T> MapTo<T>(this DataTable dt) where T : class, new()
    {
        var objProps = typeof(T).GetProperties().ToList();
        var mappings = dt.Columns.Cast<DataColumn>()
                                 .Select(x => objProps.FindIndex(y => y.Name.Equals(StswFn.NormalizeDiacritics(x.ColumnName.Replace(" ", "")), StringComparison.CurrentCultureIgnoreCase)))
                                 .ToArray();

        foreach (var row in dt.AsEnumerable())
        {
            var obj = new T();

            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i] < 0)
                    continue;

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

            yield return obj;
        }
    }

    /// <summary>
    /// Converts <see cref="System.Drawing.Bitmap"/> to <see cref="ImageSource"/>.
    /// </summary>
    public static ImageSource ToImageSource(this System.Drawing.Bitmap bmp)
    {
        var handle = bmp.GetHbitmap();
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            DeleteObject(handle);
        }
    }

    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject([In] IntPtr hObject);

    /// <summary>
    /// Converts <see cref="IEnumerable{T}"/> to <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> value) => new ObservableCollection<T>(value);

    /// <summary>
    /// Converts <see cref="IEnumerable{T}"/> to <see cref="StswBindingList{T}"/>.
    /// </summary>
    public static StswBindingList<T> ToStswBindingList<T>(this IEnumerable<T> value) where T : IStswCollectionItem => new StswBindingList<T>(value);

    /// <summary>
    /// Converts <see cref="IList{T}"/> to <see cref="StswBindingList{T}"/>.
    /// </summary>
    public static StswBindingList<T> ToStswBindingList<T>(this IList<T> value) where T : IStswCollectionItem => new StswBindingList<T>(value);

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
        if (value.Length == 0)
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

    #region Enum extensions
    /// <summary>
    /// Gets an attribute on an enum field value.
    /// </summary>
    /// <typeparam name="T">The type of the attribute you want to retrieve.</typeparam>
    /// <param name="enumVal">The enum value.</param>
    /// <returns>The attribute of type T that exists on the enum value.</returns>
    public static T? GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
    {
        var attributes = enumVal.GetType().GetMember(enumVal.ToString())[0].GetCustomAttributes(typeof(T), false);
        return attributes?.Length > 0 ? (T)attributes[0] : null;
    }

    /// <summary>
    /// Returns the next value in an enumeration, with wraparound if the end of the enumeration is reached.
    /// </summary>
    public static T GetNextValue<T>(this T value, int count = 1) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        var index = Array.IndexOf(values, value);
        var valuesLength = values.Length;
        var nextIndex = (index + count % valuesLength + valuesLength) % valuesLength;
        return values[nextIndex];
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

    #region List extensions
    /// <summary>
    /// Removes all occurrences of the specified elements from the <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the <see cref="IList{T}"/>.</typeparam>
    /// <param name="iList">The <see cref="IList{T}"/> to remove elements from.</param>
    /// <param name="itemsToRemove">The collection containing the elements to remove.</param>
    public static void RemoveBy<T>(this IList<T> iList, IEnumerable<T> itemsToRemove)
    {
        var set = new HashSet<T>(itemsToRemove);

        if (iList is not List<T> list)
        {
            int i = 0;
            while (i < iList.Count)
            {
                if (set.Contains(iList[i])) iList.RemoveAt(i);
                else i++;
            }
        }
        else list.RemoveAll(set.Contains);
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
        if (string.IsNullOrEmpty(paddingString))
            throw new ArgumentNullException(nameof(paddingString));

        if (totalWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(totalWidth), "The total width cannot be negative.");

        if (text == null || text.Length >= totalWidth)
            return text;

        while (text.Length < totalWidth)
            text = paddingString + text;

        return text[..totalWidth];
    }

    /// <summary>
    /// Returns a new string of a specified length in which the end of the current string is padded with a specified text.
    /// </summary>
    public static string? PadRight(this string text, int totalWidth, string paddingString)
    {
        if (string.IsNullOrEmpty(paddingString))
            throw new ArgumentNullException(nameof(paddingString));

        if (totalWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(totalWidth), "The total width cannot be negative.");

        if (text == null || text.Length >= totalWidth)
            return text;

        while (text.Length < totalWidth)
            text += paddingString;

        return text[0..totalWidth];
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
}
