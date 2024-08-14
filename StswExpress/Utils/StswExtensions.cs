using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
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
public static class StswExtensions
{
    #region Bool extensions
    /// <summary>
    /// Determines whether a value is between two other values (inclusive).
    /// </summary>
    /// <typeparam name="T">The type of the value to compare.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <returns><see langword="true"/> if the value is between the start and end values; otherwise, <see langword="false"/>.</returns>
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
    public static bool In<T>(this T value, params T[] parameters) => parameters == null
            ? throw new ArgumentNullException(nameof(parameters))
            : parameters.Contains(value);

    /// <summary>
    /// Checks if the given value is null, the default value for its type, or an empty object.
    /// </summary>
    /// <typeparam name="T">The type of the value to check.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="checkProperties">If true, checks if the properties of the object are null or default; otherwise, only checks if the object itself is null or default.</param>
    /// <returns>True if the value is null, the default value for its type, or an empty object; otherwise, false.</returns>
    public static bool IsNullOrDefault<T>(this T value, bool checkProperties = false)
    {
        if (value == null || Convert.IsDBNull(value))
            return true;

        var type = typeof(T);
        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
        {
            if (underlyingType.IsEnum)
                return false;

            var defaultValue = Activator.CreateInstance(underlyingType);
            return EqualityComparer<T>.Default.Equals(value, (T?)defaultValue);
        }

        if (type.IsEnum)
            return false;

        if (type.IsValueType)
        {
            if (type == typeof(bool))
                return EqualityComparer<T>.Default.Equals(value, (T)(object)false);
            else if (type.IsPrimitive)
                return EqualityComparer<T>.Default.Equals(value, default);
        }
        else
        {
            if (value is string str)
                return string.IsNullOrEmpty(str);

            if (value is IEnumerable enumerable)
                return !enumerable.GetEnumerator().MoveNext();

            if (value is object obj)
                return obj.Equals(default);

            if (checkProperties)
            {
                var properties = value.GetType().GetProperties();
                if (properties.All(p => p.GetValue(value).IsNullOrDefault(true)))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a type is a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is numeric; otherwise, <see langword="false"/>.</returns>
    public static bool IsNumericType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var numericTypes = new HashSet<Type>
        {
            typeof(sbyte), typeof(byte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double),
            typeof(decimal), typeof(nint),
            typeof(nuint)
        };

        return numericTypes.Contains(type) ||
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && numericTypes.Contains(Nullable.GetUnderlyingType(type)!));
    }
    #endregion

    #region Byte[] extensions
    /// <summary>
    /// Converts an <see cref="ImageSource"/> to a byte array.
    /// </summary>
    /// <param name="value">The <see cref="ImageSource"/> to convert.</param>
    /// <returns>A byte array representing the image.</returns>
    public static byte[] ToBytes(this ImageSource value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value is not BitmapSource bitmapSource)
            throw new ArgumentException("Value must be a BitmapSource.", nameof(value));

        var encoder = new PngBitmapEncoder();
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
    public static byte[] ToBytes(this SecureString value) => value == null
            ? throw new ArgumentNullException(nameof(value))
            : Encoding.UTF8.GetBytes(new NetworkCredential(string.Empty, value).Password);
    #endregion

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
    public static T? DeepClone<T>(this T original)
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
            return underlyingType == null ? default : null;

        if (t.IsEnum || underlyingType?.IsEnum == true)
        {
            var enumType = underlyingType ?? t;

            if (Enum.TryParse(enumType, o.ToString(), out var result))
                return result;

            return null;
        }

        try
        {
            return underlyingType == null
                ? Convert.ChangeType(o, t, CultureInfo.InvariantCulture)
                : Convert.ChangeType(o, underlyingType, CultureInfo.InvariantCulture);
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
    /// Converts a <see cref="DataTable"/> to an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to map to.</typeparam>
    /// <param name="dt">The DataTable to map.</param>
    /// <returns>An enumerable collection of objects mapped from the <see cref="DataTable"/>.</returns>
    public static IEnumerable<T> MapTo<T>(this DataTable dt) where T : class, new()
    {
        var objProps = typeof(T).GetProperties();
        var normalizedColumnNames = dt.Columns.Cast<DataColumn>()
            .Select(x => StswFn.NormalizeDiacritics(x.ColumnName.Replace(" ", "")))
            .ToArray();
        var mappings = normalizedColumnNames
            .Select(x => Array.FindIndex(objProps, prop => prop.Name.Equals(x, StringComparison.CurrentCultureIgnoreCase)))
            .ToArray();

        foreach (var row in dt.AsEnumerable())
        {
            var obj = new T();

            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i] < 0)
                    continue;

                var propertyInfo = objProps[mappings[i]];
                if (propertyInfo != null)
                {
                    try
                    {
                        var value = row[i] == DBNull.Value ? null : row[i];
                        propertyInfo.SetValue(obj, value?.ConvertTo(propertyInfo.PropertyType), null);
                    }
                    catch
                    {
                        // Optionally, log the exception or handle it as needed
                        continue;
                    }
                }
            }

            yield return obj;
        }
    }

    /// <summary>
    /// Converts a <see cref="DataTable"/> to an <see cref="IEnumerable{T}"/> and supports nested class property mappings.
    /// </summary>
    /// <typeparam name="T">The type of objects to map to.</typeparam>
    /// <param name="dt">The DataTable to map.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column names.</param>
    /// <returns>An enumerable collection of objects mapped from the <see cref="DataTable"/>.</returns>
    public static IEnumerable<T> MapTo<T>(this DataTable dt, char delimiter) where T : class, new()
    {
        var normalizedColumnNames = dt.Columns.Cast<DataColumn>()
            .Select(x => StswFn.NormalizeDiacritics(x.ColumnName.Replace(" ", "")))
            .ToArray();

        var propCache = StswMapping.CacheProperties(typeof(T), normalizedColumnNames, delimiter);

        foreach (var row in dt.AsEnumerable())
        {
            var obj = new T();
            StswMapping.MapRowToObject(obj, row, normalizedColumnNames, delimiter, propCache);
            yield return obj;
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
    /// Converts a <see cref="System.Drawing.Bitmap"/> to an <see cref="ImageSource"/>.
    /// </summary>
    /// <param name="bmp">The bitmap to convert.</param>
    /// <returns>The converted <see cref="ImageSource"/>.</returns>
    public static ImageSource ToImageSource(this System.Drawing.Bitmap bmp)
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject([In] IntPtr hObject);

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

    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> to an <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection.</typeparam>
    /// <param name="value">The enumerable to convert.</param>
    /// <returns>The converted <see cref="ObservableCollection{T}"/>.</returns>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> value) => new(value);

    /// <summary>
    /// Converts a <see cref="Type"/> to a <see cref="SqlDbType"/>.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    /// <returns>The corresponding <see cref="SqlDbType"/>, or null if no matching type is found.</returns>
    public static SqlDbType? InferSqlDbType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType switch
        {
            { } t when t == typeof(byte) || t == typeof(sbyte) => SqlDbType.TinyInt,
            { } t when t == typeof(short) || t == typeof(ushort) => SqlDbType.SmallInt,
            { } t when t == typeof(int) || t == typeof(uint) => SqlDbType.Int,
            { } t when t == typeof(long) || t == typeof(ulong) => SqlDbType.BigInt,
            { } t when t == typeof(float) => SqlDbType.Real,
            { } t when t == typeof(double) => SqlDbType.Float,
            { } t when t == typeof(decimal) => SqlDbType.Decimal,
            { } t when t == typeof(bool) => SqlDbType.Bit,
            { } t when t == typeof(string) => SqlDbType.NVarChar,
            { } t when t == typeof(char) => SqlDbType.NChar,
            { } t when t == typeof(Guid) => SqlDbType.UniqueIdentifier,
            { } t when t == typeof(DateTime) => SqlDbType.DateTime,
            { } t when t == typeof(DateTimeOffset) => SqlDbType.DateTimeOffset,
            { } t when t == typeof(byte[]) => SqlDbType.VarBinary,
            _ => null
        };
    }

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
    /// Converts a byte array to a <see cref="BitmapImage"/>.
    /// </summary>
    /// <param name="value">The byte array to convert.</param>
    /// <returns>The converted <see cref="BitmapImage"/>, or null if the byte array is empty.</returns>
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
    /// Converts a <see cref="Color"/> to a <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="value">The <see cref="Color"/> to convert.</param>
    /// <returns>The converted <see cref="System.Drawing.Color"/>.</returns>
    public static System.Drawing.Color ToDrawingColor(this Color value) => System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);

    /// <summary>
    /// Converts a <see cref="System.Drawing.Color"/> to a <see cref="Color"/>.
    /// </summary>
    /// <param name="value">The <see cref="System.Drawing.Color"/> to convert.</param>
    /// <returns>The converted <see cref="Color"/>.</returns>
    public static Color ToMediaColor(this System.Drawing.Color value) => Color.FromArgb(value.A, value.R, value.G, value.B);

    /// <summary>
    /// Converts a <see cref="Color"/> to an HTML color string.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The HTML color string representation of the color.</returns>
    public static string ToHtml(this Color color) => new ColorConverter().ConvertToString(color) ?? string.Empty;
    #endregion

    #region DateTime extensions
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
    /// <returns>The attribute of type T that exists on the enum value, or null if no such attribute is found.</returns>
    public static T? GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
    {
        var memberInfo = enumVal.GetType().GetMember(enumVal.ToString()).FirstOrDefault();
        if (memberInfo == null) return null;

        return memberInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
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
        int index = Array.IndexOf(values, value);
        int nextIndex = index + count;

        if (wrapAround)
            nextIndex = (nextIndex + values.Length) % values.Length;
        else
            nextIndex = Math.Min(nextIndex, values.Length - 1);

        return values[nextIndex];
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

        foreach (var i in type.GetInterfaces())
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                innerType = i.GetGenericArguments().Single();
                return true;
            }

        return false;
    }

    /// <summary>
    /// Applies a specified action to each element of the IList, allowing modification of 
    /// individual properties within each element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the IList.</typeparam>
    /// <param name="list">The IList on which the action will be performed.</param>
    /// <param name="modifier">An action that defines the modification to be applied to each element.</param>
    /// <returns>The same IList after the modifications have been applied to its elements.</returns>
    /// <remarks>
    /// This method modifies the elements of the IList in-place, meaning that it does not 
    /// create a new collection or new elements, but instead applies the provided action to 
    /// each existing element in the IList. 
    /// It is particularly useful when you need to update specific properties of objects 
    /// within a collection without altering the entire object or creating a new collection.
    /// </remarks>
    public static IList<T> ModifyEach<T>(this IList<T> list, Action<T> modifier)
    {
        foreach (var item in list)
            modifier(item);
        return list;
    }

    /// <summary>
    /// Removes all occurrences of the specified elements from an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to remove elements from.</param>
    /// <param name="itemsToRemove">The collection containing the elements to remove.</param>
    /// <returns>A new collection with the specified elements removed.</returns>
    public static IEnumerable<T> Remove<T>(this IEnumerable<T> collection, IEnumerable<T> itemsToRemove) => collection.Where(item => !new HashSet<T>(itemsToRemove).Contains(item));

    /// <summary>
    /// Removes all occurrences of the specified elements from the <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="iList">The list to remove elements from.</param>
    /// <param name="itemsToRemove">The collection containing the elements to remove.</param>
    public static void Remove<T>(this IList<T> iList, IEnumerable<T> itemsToRemove)
    {
        var set = new HashSet<T>(itemsToRemove);

        if (iList is List<T> list)
            list.RemoveAll(set.Contains);
        else
            for (int i = iList.Count - 1; i >= 0; i--)
                if (set.Contains(iList[i]))
                    iList.RemoveAt(i);
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
        return source == null
            ? throw new ArgumentNullException(nameof(source))
            : source.Select(item => EqualityComparer<T>.Default.Equals(item, oldValue) ? newValue : item);
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

        for (int i = 0; i < source.Count; i++)
            if (EqualityComparer<T>.Default.Equals(source[i], oldValue))
                source[i] = newValue;
    }
    #endregion

    #region Sql extensions
    /// <summary>
    /// Adds a list of parameters to the <see cref="SqlCommand"/> by replacing the specified parameter name in the SQL query with the list of values.
    /// If the list is null or empty, replaces the parameter with NULL in the SQL query.
    /// </summary>
    /// <param name="sqlCommand">The <see cref="SqlCommand"/> object.</param>
    /// <param name="parameterName">The parameter name to be replaced in the SQL query.</param>
    /// <param name="list">The list of values to be added as parameters.</param>
    public static void ParametersAddList(this SqlCommand sqlCommand, string parameterName, IList? list)
    {
        //TODO - limit to max. 50 elements in list (to prevent idiocy)

        ArgumentNullException.ThrowIfNull(sqlCommand);
        if (string.IsNullOrEmpty(parameterName))
            throw new ArgumentException("Parameter name cannot be null or empty.", nameof(parameterName));

        if (list == null || !IsListType(list.GetType(), out var innerType) || list.Count == 0)
        {
            sqlCommand.CommandText = Regex.Replace(sqlCommand.CommandText, $@"{Regex.Escape(parameterName)}(?!\w)", "NULL", RegexOptions.IgnoreCase);
            return;
        }

        var parameterNames = new StringBuilder();
        for (var i = 0; i < list?.Count; i++)
        {
            var paramName = $"{parameterName}{i}";
            sqlCommand.Parameters.Add(paramName, innerType!.InferSqlDbType()!.Value).Value = list[i] ?? DBNull.Value;
            if (i > 0) parameterNames.Append(',');
            parameterNames.Append(paramName);
        }

        sqlCommand.CommandText = Regex.Replace(sqlCommand.CommandText, $@"{Regex.Escape(parameterName)}(?!\w)", parameterNames.ToString(), RegexOptions.IgnoreCase);
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

    /// <summary>
    /// Gets the value of a property by name from an object.
    /// </summary>
    /// <param name="obj">The object from which to get the property value.</param>
    /// <param name="propertyName">The name of the property whose value is to be retrieved.</param>
    /// <returns>The value of the specified property, or null if the property is not found.</returns>
    public static object? GetPropertyValue(this object obj, string propertyName) => obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
    #endregion
}
