using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Clones a <see cref="BindingBase"/> object.
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
                        ValidatesOnExceptions = multiBinding.ValidatesOnDataErrors,
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
    /// Creates a deep copy of the specified object.
    /// </summary>
    /// <typeparam name="T">The type of the object being cloned.</typeparam>
    /// <param name="original">The original object to clone.</param>
    /// <returns>A deep copy of the original object. The returned object and any sub-objects are entirely independent of the original.</returns>
    /// <remarks>
    /// This method handles cloning of primitive types, complex object graphs, and collections. It uses reflection to dynamically create a copy
    /// of the object, ensuring that all nested objects and collections are also deeply cloned. It does not handle circular references which can lead to stack overflow.
    /// </remarks>
    [Obsolete("Bugged.")]
    public static T? DeepClone<T>(this T original) where T : class
    {
        if (original == null)
            return default;

        var typeToReflect = original.GetType();
        if (StswClone.IsPrimitive(typeToReflect))
            return original;

        var cloneObject = Activator.CreateInstance<T>();
        StswClone.CopyProperties(original, cloneObject, typeToReflect);
        StswClone.CopyFields(original, cloneObject, typeToReflect);

        return cloneObject;
    }

    /// <summary>
    /// Attempts to clone each item in an <see cref="IEnumerable"/> and returns a new IEnumerable with the cloned items.
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
    /// Converts an <see cref="object"/> to a specified type.
    /// </summary>
    /// <param name="o">The object to convert.</param>
    /// <param name="t">The type to convert to.</param>
    /// <returns>The converted <see cref="object"/> of the specified type.</returns>
    public static object? ConvertTo(this object? o, Type t)
    {
        var underlyingType = Nullable.GetUnderlyingType(t);

        if (o == null || o == DBNull.Value)
            return underlyingType != null ? null : (t.IsValueType ? Activator.CreateInstance(t) : null);

        var targetType = underlyingType ?? t;
        if (targetType.IsEnum)
        {
            if (Enum.TryParse(targetType, o.ToString(), out var result))
                return result;
            return null;
        }

        try
        {
            return Convert.ChangeType(o, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts an <see cref="object"/> to a specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="o">The object to convert.</param>
    /// <returns>The converted <see cref="object"/> of type <see cref="{T}"/>.</returns>
    public static T? ConvertTo<T>(this object o) => o.ConvertTo(typeof(T)) is T tResult ? tResult : default;

    /// <summary>
    /// Converts a <see cref="Type"/> to a <see cref="SqlDbType"/>.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    /// <returns>The corresponding <see cref="SqlDbType"/>, or null if no matching type is found.</returns>
    public static SqlDbType InferSqlDbType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var underlyingType = Nullable.GetUnderlyingType(type) ?? (type.IsEnum ? Enum.GetUnderlyingType(type) : type);

        return TypeToSqlDbTypeMap.TryGetValue(underlyingType, out var sqlDbType) ? sqlDbType : SqlDbType.NVarChar;
    }
    private static readonly Dictionary<Type, SqlDbType> TypeToSqlDbTypeMap = new()
    {
        { typeof(byte), SqlDbType.TinyInt },
        { typeof(sbyte), SqlDbType.TinyInt },
        { typeof(short), SqlDbType.SmallInt },
        { typeof(ushort), SqlDbType.SmallInt },
        { typeof(int), SqlDbType.Int },
        { typeof(uint), SqlDbType.Int },
        { typeof(long), SqlDbType.BigInt },
        { typeof(ulong), SqlDbType.BigInt },
        { typeof(float), SqlDbType.Real },
        { typeof(double), SqlDbType.Float },
        { typeof(decimal), SqlDbType.Decimal },
        { typeof(bool), SqlDbType.Bit },
        { typeof(string), SqlDbType.NVarChar },
        { typeof(char), SqlDbType.NChar },
        { typeof(Guid), SqlDbType.UniqueIdentifier },
        { typeof(DateTime), SqlDbType.DateTime },
        { typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
        { typeof(byte[]), SqlDbType.VarBinary }
    };

    /// <summary>
    /// Converts a <see cref="DataTable"/> to an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to map to.</typeparam>
    /// <param name="dt">The DataTable to map.</param>
    /// <returns>An enumerable collection of objects mapped from the <see cref="DataTable"/>.</returns>
    public static IEnumerable<T?> MapTo<T>(this DataTable dt)
    {
        var type = typeof(T);

        if (!type.IsClass || type == typeof(byte[]) || type == typeof(string))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo<T?>();
        }
        else
        {
            var objProps = type.GetProperties();
            var mappings = new Dictionary<int, PropertyInfo>();

            for (var i = 0; i < dt.Columns.Count; i++)
            {
                var columnName = StswFn.NormalizeDiacritics(dt.Columns[i].ColumnName.Replace(" ", ""));
                var prop = objProps.FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));

                if (prop != null && prop.CanWrite)
                    mappings.Add(i, prop);
            }

            foreach (var row in dt.AsEnumerable())
            {
                var obj = Activator.CreateInstance<T>();

                foreach (var kvp in mappings)
                {
                    var colIndex = kvp.Key;
                    var prop = kvp.Value;

                    try
                    {
                        prop.SetValue(obj, row[colIndex].ConvertTo(prop.PropertyType));
                    }
                    catch
                    {
                        // Optional logging or handling
                    }
                }

                yield return obj;
            }
        }
    }

    /// <summary>
    /// Converts a <see cref="DataTable"/> to an <see cref="IEnumerable{T}"/> and supports nested class property mappings.
    /// </summary>
    /// <typeparam name="T">The type of objects to map to.</typeparam>
    /// <param name="dt">The DataTable to map.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column names.</param>
    /// <returns>An enumerable collection of objects mapped from the <see cref="DataTable"/>.</returns>
    public static IEnumerable<T?> MapTo<T>(this DataTable dt, char delimiter)
    {
        var type = typeof(T);

        if (!type.IsClass || type == typeof(string))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo<T>();
        }
        else
        {
            var normalizedColumnNames = dt.Columns.Cast<DataColumn>()
                                                  .Select(x => StswFn.NormalizeDiacritics(x.ColumnName.Replace(" ", "")))
                                                  .ToArray();

            var propCache = StswMapping.CacheProperties(typeof(T), normalizedColumnNames, delimiter);

            foreach (var row in dt.AsEnumerable())
            {
                var obj = Activator.CreateInstance<T>();
                StswMapping.MapRowToObject(obj, row, normalizedColumnNames, delimiter, propCache);
                yield return obj;
            }
        }
    }

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
    /// Converts a <see cref="SecureString"/> to a byte array.
    /// </summary>
    /// <param name="value">The <see cref="SecureString"/> to convert.</param>
    /// <returns>A byte array representing the secure string.</returns>
    public static byte[] ToBytes(this SecureString value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.SecureStringToGlobalAllocUnicode(value);
            var length = value.Length;
            var bytes = new byte[length * sizeof(char)];

            Marshal.Copy(ptr, bytes, 0, bytes.Length);

            return Encoding.Convert(Encoding.Unicode, Encoding.UTF8, bytes);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(ptr);
        }
    }

    /// <summary>
    /// Converts a collection of items to a <see cref="DataTable"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection.</typeparam>
    /// <param name="data">The collection of items to convert.</param>
    /// <returns>A <see cref="DataTable"/> containing the data from the collection.</returns>
    public static DataTable ToDataTable<T>(this IEnumerable<T> data)
    {
        var dataTable = new DataTable(typeof(T).Name);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
            dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);

        foreach (var item in data)
        {
            var values = new object[properties.Length];
            for (int i = 0; i < properties.Length; i++)
                values[i] = properties[i].GetValue(item) ?? DBNull.Value;
            dataTable.Rows.Add(values);
        }

        return dataTable;
    }

    /// <summary>
    /// Converts a collection to a dictionary, safely handling duplicate keys by ignoring subsequent duplicates.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="source">The collection to convert.</param>
    /// <param name="keySelector">A function to extract keys from the elements.</param>
    /// <param name="valueSelector">A function to extract values from the elements.</param>
    /// <returns>A dictionary containing keys and values selected from the collection, with duplicate keys safely ignored.</returns>
    /// <remarks>
    /// This method is useful when you want to convert a collection to a dictionary but cannot guarantee that the keys will be unique.
    /// </remarks>
    public static Dictionary<TKey, TValue> ToDictionarySafely<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector) where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>();

        foreach (var item in source)
            dictionary.TryAdd(keySelector(item), valueSelector(item));

        return dictionary;
    }

    /// <summary>
    /// Converts a <see cref="System.Drawing.Bitmap"/> to an <see cref="ImageSource"/>.
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

    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> to an <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection.</typeparam>
    /// <param name="value">The enumerable to convert.</param>
    /// <returns>The converted <see cref="ObservableCollection{T}"/>.</returns>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> value) => new(value);

    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="StswBindingList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="value">The enumerable to convert.</param>
    /// <returns>The converted <see cref="StswBindingList{T}"/>.</returns>
    public static StswBindingList<T> ToStswBindingList<T>(this IEnumerable<T> value) where T : IStswCollectionItem => new(value);

    /// <summary>
    /// Converts an <see cref="IDictionary{TKey, TValue}"/> to a <see cref="StswDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the dictionary keys.</typeparam>
    /// <typeparam name="T2">The type of the dictionary values.</typeparam>
    /// <param name="value">The dictionary to convert.</param>
    /// <returns>The converted <see cref="StswDictionary{TKey, TValue}"/>.</returns>
    public static StswDictionary<T1, T2> ToStswDictionary<T1, T2>(this IDictionary<T1, T2> value) where T1 : notnull => new(value);
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
    /// Converts a <see cref="System.Drawing.Color"/> to a <see cref="Color"/>.
    /// </summary>
    /// <param name="value">The <see cref="System.Drawing.Color"/> to convert.</param>
    /// <returns>The converted <see cref="Color"/>.</returns>
    public static Color ToMediaColor(this System.Drawing.Color value) => Color.FromArgb(value.A, value.R, value.G, value.B);
    #endregion

    #region DateTime extensions
    /// <summary>
    /// Finds the next occurrence of a specified day of the week after a given date.
    /// </summary>
    /// <param name="from">The starting date.</param>
    /// <param name="dayOfWeek">The day of the week to find.</param>
    /// <returns>The next date that falls on the specified day of the week.</returns>
    /// <remarks>
    /// This method is useful for scheduling or finding specific days, such as the next Monday after a given date.
    /// </remarks>
    public static DateTime Next(this DateTime from, DayOfWeek dayOfWeek)
    {
        var start = (int)from.DayOfWeek;
        var target = (int)dayOfWeek;

        var daysToAdd = (target - start + 7) % 7;
        if (daysToAdd == 0) daysToAdd = 7;

        return from.AddDays(daysToAdd);
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> to a Unix timestamp.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
    /// <returns>A long value representing the number of seconds since the Unix epoch (January 1, 1970).</returns>
    public static long ToUnixTimeSeconds(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    #endregion

    #region Enum extensions
    /// <summary>
    /// Gets an attribute of a specified type on an enum field value.
    /// </summary>
    /// <typeparam name="T">The type of the attribute to retrieve.</typeparam>
    /// <param name="enumVal">The enum value.</param>
    /// <returns>The attribute of type <see cref="{T}"/> that exists on the enum value, or <see langword="null"/> if no such attribute is found.</returns>
    public static T? GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
    {
        var memberInfo = enumVal.GetType().GetMember(enumVal.ToString());

        if (memberInfo.Length == 0)
            return null;

        return memberInfo[0].GetCustomAttribute<T>(false);
    }

    /// <summary>
    /// Retrieves the description attribute from an enum value, if present.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>The description attribute of the enum value, or the enum value's name if no description is found.</returns>
    /// <remarks>
    /// This method is useful when you want to display user-friendly descriptions of enum values in the UI or logs.
    /// </remarks>
    public static string GetDescription(this Enum enumVal)
    {
        var field = enumVal.GetType().GetField(enumVal.ToString());
        if (field == null)
            return enumVal.ToString();

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? enumVal.ToString();
    }

    /// <summary>
    /// Returns the next value in an enumeration, optionally with wraparound if the end of the enumeration is reached.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="value">The current enum value.</param>
    /// <param name="count">The number of steps to move forward in the enumeration.</param>
    /// <param name="wrapAround">Whether to wrap around to the first value when the end of the enumeration is reached.</param>
    /// <returns>The next enum value. If wrapAround is <see langword="false"/> and the end is reached, returns the last enum value.</returns>
    public static T GetNextValue<T>(this T value, int count = 1, bool wrapAround = true) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        int length = values.Length;
        int index = Array.IndexOf(values, value);

        if (wrapAround)
        {
            int nextIndex = (index + count % length + length) % length;
            return values[nextIndex];
        }
        else
        {
            int nextIndex = Math.Clamp(index + count, 0, length - 1);
            return values[nextIndex];
        }
    }
    #endregion

    #region Exception extensions
    /// <summary>
    /// Gets the innermost exception of an exception.
    /// </summary>
    /// <param name="ex">The exception from which to get the innermost exception.</param>
    /// <returns>The innermost <see cref="Exception"/>.</returns>
    public static Exception GetInnermostException(this Exception ex)
    {
        while (ex.InnerException != null)
            ex = ex.InnerException;

        return ex;
    }
    #endregion

    #region List extensions
    /// <summary>
    /// Adds an item to a list if the list does not already contain the item.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to add the item to.</param>
    /// <param name="item">The item to add.</param>
    public static void AddIfNotContains<T>(this IList<T> list, T item)
    {
        if (!list.Contains(item))
            list.Add(item);
    }

    /// <summary>
    /// Splits a collection into smaller batches of the specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to be split into batches.</param>
    /// <param name="size">The size of each batch.</param>
    /// <returns>An enumerable collection of batches, where each batch is a list containing the specified number of elements.</returns>
    /// <remarks>
    /// This method can be useful when processing large collections in smaller chunks, such as when paging through data or processing data in smaller, manageable parts.
    /// </remarks>
    public static IEnumerable<IList<T>> Batch<T>(this IEnumerable<T> source, int size)
    {
        var batch = new List<T>(size);

        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == size)
            {
                yield return batch;
                batch = new(size);
            }
        }

        if (batch.Count != 0)
            yield return batch;
    }

    /// <summary>
    /// Applies a specified action to each element of the <see cref="IList{}"/>, allowing modification of 
    /// individual properties within each element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the <see cref="IList{}"/>.</typeparam>
    /// <param name="list">The <see cref="IList{}"/> on which the action will be performed.</param>
    /// <param name="modifier">An action that defines the modification to be applied to each element.</param>
    /// <returns>The same <see cref="IList{}"/> after the modifications have been applied to its elements.</returns>
    /// <remarks>
    /// This method modifies the elements of the <see cref="IList{}"/> in-place, meaning that it does not 
    /// create a new collection or new elements, but instead applies the provided action to 
    /// each existing element in the <see cref="IList{}"/>. 
    /// It is particularly useful when you need to update specific properties of objects 
    /// within a collection without altering the entire object or creating a new collection.
    /// </remarks>
    public static IList<T> ModifyEach<T>(this IList<T> list, Action<T> modifier)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));
        ArgumentNullException.ThrowIfNull(modifier, nameof(modifier));

        if (list.Count == 0)
            return list;

        foreach (var item in list)
            modifier(item);

        return list;
    }

    /// <summary>
    /// Removes all occurrences of the specified elements from the <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to remove elements from.</param>
    /// <param name="items">The collection containing the elements to remove.</param>
    public static void Remove<T>(this IList<T> list, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        var set = new HashSet<T>(items);
        if (set.Count == 0)
            return;

        if (list is List<T> strongList)
        {
            strongList.RemoveAll(set.Contains);
        }
        else
        {
            for (var i = list.Count - 1; i >= 0; i--)
                if (set.Contains(list[i]))
                    list.RemoveAt(i);
        }
    }

    /// <summary>
    /// Replaces all occurrences of a specified value in an <see cref="IEnumerable{T}"/> with another value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="oldValue">The value to replace.</param>
    /// <param name="newValue">The value to replace with.</param>
    /// <returns>A new collection with the specified value replaced.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source collection is null.</exception>
    public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T oldValue, T newValue)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        return source.Select(item => EqualityComparer<T>.Default.Equals(item, oldValue) ? newValue : item);
    }

    /// <summary>
    /// Replaces all occurrences of a specified value in an <see cref="IList{T}"/> with another value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="oldValue">The value to replace.</param>
    /// <param name="newValue">The value to replace with.</param>
    /// <exception cref="ArgumentNullException">Thrown when the source collection is null.</exception>
    public static void Replace<T>(this IList<T> source, T oldValue, T newValue)
    {
        ArgumentNullException.ThrowIfNull(source);

        for (var i = 0; i < source.Count; i++)
            if (EqualityComparer<T>.Default.Equals(source[i], oldValue))
                source[i] = newValue;
    }

    /// <summary>
    /// Randomizes the order of elements in a list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    /// <remarks>
    /// This method can be useful in scenarios where random order is needed, such as randomizing test cases or mixing elements in non-deterministic processes.
    /// </remarks>
    public static void Shuffle<T>(this IList<T> list)
    {
        var rng = new Random();
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
    #endregion

    #region Logical extensions
    /// <summary>
    /// Checks if a value is within a specified range, inclusive.
    /// </summary>
    /// <typeparam name="T">The type of the value to compare.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <returns><see langword="true"/> if the value is within the range; otherwise, <see langword="false"/>.</returns>
    public static bool Between<T>(this T? value, T? start, T? end)
    {
        var comparer = Comparer<T>.Default;
        return comparer.Compare(value, start) >= 0 && comparer.Compare(value, end) <= 0;
    }

    /// <summary>
    /// Determines whether a value is contained in a collection.
    /// </summary>
    /// <typeparam name="T">The type of the value to check.</typeparam>
    /// <param name="value">The value to check for.</param>
    /// <param name="collection">The collection to check in.</param>
    /// <returns><see langword="true"/> if the value is contained in the collection; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is null.</exception>
    public static bool In<T>(this T value, IEnumerable<T> collection) => collection == null
            ? throw new ArgumentNullException(nameof(collection))
            : collection.Contains(value);

    /// <summary>
    /// Determines whether a value is contained in a parameters.
    /// </summary>
    /// <typeparam name="T">The type of the value to check.</typeparam>
    /// <param name="value">The value to check for.</param>
    /// <param name="parameters">The parameters to check in.</param>
    /// <returns><see langword="true"/> if the value is contained in the parameters; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is null.</exception>
    public static bool In<T>(this T value, params T[] parameters) => parameters == null
            ? throw new ArgumentNullException(nameof(parameters))
            : parameters.Contains(value);

    /// <summary>
    /// Checks if a type is a list type and retrieves the inner type if it is.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="innerType">The inner type if the type is a list type.</param>
    /// <returns><see langword="true"/> if the type is a list type; otherwise, <see langword="false"/>.</returns>
    internal static bool IsListType(this Type type, out Type? innerType)
    {
        ArgumentNullException.ThrowIfNull(type);
        innerType = null;

        if (type == typeof(string))
            return false;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            innerType = type.GetGenericArguments().Single();
            return true;
        }

        if (type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) is Type iType)
        {
            innerType = iType.GetGenericArguments().Single();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the given value is null, the default value for its type, or a value that indicates absence of meaningful data.
    /// </summary>
    /// <typeparam name="T">The type of the value to check.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if the value is null, the default value for its type, or indicates absence of data; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullOrDefault<T>(this T? value)
    {
        if (value == null || Convert.IsDBNull(value))
            return true;

        var type = typeof(T);

        if (Nullable.GetUnderlyingType(type) != null)
        {
            var underlyingType = Nullable.GetUnderlyingType(type)!;
            return EqualityComparer<T>.Default.Equals(value, (T?)Activator.CreateInstance(underlyingType));
        }

        if (type == typeof(bool))
            return false;

        if (type == typeof(string))
            return string.IsNullOrEmpty(value as string);

        if (type.IsEnum)
            return EqualityComparer<T>.Default.Equals(value, default);

        if (value is IEnumerable enumerable)
            return !enumerable.GetEnumerator().MoveNext();

        if (type.IsClass)
            return value.IsSimilarTo((T?)Activator.CreateInstance(type));

        return EqualityComparer<T>.Default.Equals(value, default);
    }

    /// <summary>
    /// Determines whether a type is a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is numeric; otherwise, <see langword="false"/>.</returns>
    public static bool IsNumericType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            type = underlyingType;

        if (type == typeof(nint) || type == typeof(nuint))
            return true;

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Byte or
            TypeCode.SByte or
            TypeCode.UInt16 or
            TypeCode.UInt32 or
            TypeCode.UInt64 or
            TypeCode.Int16 or
            TypeCode.Int32 or
            TypeCode.Int64 or
            TypeCode.Decimal or
            TypeCode.Double or
            TypeCode.Single => true,
            _ => false,
        };
    }

    /// <summary>
    /// Compares two objects of the same type by checking if all their public properties are equal.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="objA">The first object to compare.</param>
    /// <param name="objB">The second object to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both objects are either null or have identical public property values;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method performs a shallow comparison of the public properties of both objects. 
    /// It compares the values of public properties using <see cref="EqualityComparer{T}.Equals"/> method.
    /// The method skips indexer properties during the comparison.
    /// </remarks>
    public static bool IsSimilarTo<T>(this T objA, T objB)
    {
        if (objA == null && objB == null)
            return true;
        else if (objA == null || objB == null)
            return false;

        var type = typeof(T);

        foreach (var property in type.GetProperties())
        {
            if (property.GetIndexParameters().Length > 0)
                continue;

            var valueA = property.GetValue(objA);
            var valueB = property.GetValue(objB);

            if (!EqualityComparer<object>.Default.Equals(valueA, valueB))
                return false;
        }

        return true;
    }
    #endregion

    #region Numeric extensions
    /// <summary>
    /// Shifts the value by the specified step, with optional looping and boundary conditions.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="step">The step value for shifting.</param>
    /// <param name="max">The maximum possible value (exclusive upper bound).</param>
    /// <param name="isLoopingAllowed">Specifies whether looping is allowed when shifting past boundaries.</param>
    /// <returns>The new shifted value, respecting looping and boundary conditions.</returns>
    public static int ShiftBy(this int value, int step, int max, bool isLoopingAllowed = true)
    {
        if (max <= 0)
            return -1;

        int result = (value + step) % max;
        if (result < 0)
            result += max;

        if (!isLoopingAllowed)
        {
            if (result >= max)
                return max - 1;

            if (result < 0)
                return 0;
        }

        return result;
    }
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
            var user = new WindowsIdentity(processHandle).Name;
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

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr hObject);
    #endregion

    #region SQL extensions
    /// <summary>
    /// Adds a list of parameters to the <see cref="SqlCommand"/> by replacing the specified parameter name in the SQL query with the list of values.
    /// If the list is null or empty, replaces the parameter with NULL in the SQL query.
    /// </summary>
    /// <param name="sqlCommand">The <see cref="SqlCommand"/> object.</param>
    /// <param name="parameterName">The parameter name to be replaced in the SQL query.</param>
    /// <param name="list">The list of values to be added as parameters.</param>
    /// <exception cref="ArgumentException">Thrown when the list contains more than 20 elements.</exception>
    public static void ParametersAddList(this SqlCommand sqlCommand, string parameterName, IList? list)
    {
        ArgumentNullException.ThrowIfNull(sqlCommand);
        if (string.IsNullOrEmpty(parameterName))
            throw new ArgumentException("Parameter name cannot be null or empty.", nameof(parameterName));

        const int maxListSize = 20;
        if (list?.Count > maxListSize)
            throw new ArgumentException($"The list contains more than {maxListSize} elements, which exceeds the allowed limit.", nameof(list));

        string replacementValue;

        if (list == null || list.Count == 0 || !IsListType(list.GetType(), out var innerType) || innerType == null)
        {
            replacementValue = "NULL";
        }
        else
        {
            var sqlDbType = innerType.InferSqlDbType();
            replacementValue = string.Join(',', Enumerable.Range(0, list.Count).Select(i =>
            {
                var paramName = $"{parameterName}{i}";
                sqlCommand.Parameters.Add(paramName, sqlDbType).Value = list[i] ?? DBNull.Value;
                return paramName;
            }));
        }

        sqlCommand.CommandText = Regex.Replace(sqlCommand.CommandText, $@"{Regex.Escape(parameterName)}(?!\w)", replacementValue, RegexOptions.IgnoreCase);
    }
    #endregion

    #region Text extensions
    /// <summary>
    /// Capitalizes the first letter of the string and converts the rest of the characters to lowercase.
    /// </summary>
    /// <param name="text">The string to capitalize.</param>
    /// <returns>A string with the first letter capitalized and the rest in lowercase.</returns>
    public static string Capitalize(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (text.Length == 1)
            return char.ToUpper(text[0]).ToString();

        return $"{char.ToUpper(text[0])}{text[1..].ToLower()}";
    }

    /// <summary>
    /// Returns a new string that is right-aligned in a new string of a specified length by padding it on the left with a specified text string.
    /// </summary>
    /// <param name="text">The string to pad.</param>
    /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    /// <param name="paddingString">The string to pad with.</param>
    /// <returns>A new string that is equivalent to the original string, but right-aligned and padded on the left with paddingString characters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when paddingString is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when totalWidth is less than zero.</exception>
    public static string? PadLeft(this string text, int totalWidth, string paddingString)
    {
        if (string.IsNullOrEmpty(paddingString))
            throw new ArgumentNullException(nameof(paddingString));

        if (totalWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(totalWidth), "The total width cannot be negative.");

        if (text == null || text.Length >= totalWidth)
            return text;

        var sb = new StringBuilder(totalWidth);
        while (sb.Length + text.Length < totalWidth)
            sb.Append(paddingString);
        sb.Append(text);

        return sb.ToString(0, totalWidth);
    }

    /// <summary>
    /// Returns a new string that is left-aligned in a new string of a specified length by padding it on the right with a specified text string.
    /// </summary>
    /// <param name="text">The string to pad.</param>
    /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    /// <param name="paddingString">The string to pad with.</param>
    /// <returns>A new string that is equivalent to the original string, but left-aligned and padded on the right with paddingString characters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when paddingString is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when totalWidth is less than zero.</exception>
    public static string? PadRight(this string text, int totalWidth, string paddingString)
    {
        if (string.IsNullOrEmpty(paddingString))
            throw new ArgumentNullException(nameof(paddingString));

        if (totalWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(totalWidth), "The total width cannot be negative.");

        if (text == null || text.Length >= totalWidth)
            return text;

        var sb = new StringBuilder(text);
        while (sb.Length < totalWidth)
            sb.Append(paddingString);

        return sb.ToString(0, totalWidth);
    }

    /// <summary>
    /// Removes the specified string from the end of the current string instance.
    /// </summary>
    /// <param name="source">The string to trim.</param>
    /// <param name="value">The string to remove from the end.</param>
    /// <returns>A new string that is equivalent to the original string but without the specified value at the end.</returns>
    public static string TrimEnd(this string source, string value)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            return source;

        return source.EndsWith(value) ? source.Remove(source.LastIndexOf(value)) : source;
    }

    /// <summary>
    /// Removes the specified string from the start of the current string instance.
    /// </summary>
    /// <param name="source">The string to trim.</param>
    /// <param name="value">The string to remove from the start.</param>
    /// <returns>A new string that is equivalent to the original string but without the specified value at the start.</returns>
    public static string TrimStart(this string source, string value)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            return source;

        return source.StartsWith(value) ? source[value.Length..] : source;
    }
    #endregion

    #region Universal extensions
    /// <summary>
    /// Gets the value of a property by name from an object.
    /// </summary>
    /// <param name="obj">The object from which to get the property value.</param>
    /// <param name="propertyName">The name of the property whose value is to be retrieved.</param>
    /// <returns>The value of the specified property, or null if the property is not found.</returns>
    public static object? GetPropertyValue(this object obj, string propertyName) => obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
    #endregion

    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject([In] IntPtr hObject);
}
