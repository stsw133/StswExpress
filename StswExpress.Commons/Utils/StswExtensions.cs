﻿using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;

namespace StswExpress.Commons;
/// <summary>
/// Collection of extension methods for various types and objects. These methods simplify common tasks and provide additional functionality beyond what is available in the standard WPF API.
/// </summary>
public static partial class StswExtensions
{
    #region Convert extensions
    /// <summary>
    /// Converts an object to a specified type, supporting nullable types, enums, and primitive conversions.
    /// If conversion fails, it returns the default value of the target type.
    /// </summary>
    /// <param name="o">The object to convert. If <see langword="null"/>, the <see langword="null"/> is returned.</param>
    /// <param name="t">The target type to convert to.</param>
    /// <returns>The converted object of type <paramref name="t"/>, or <see langword="null"/> if conversion fails.</returns>

    [StswInfo(null)]
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
    /// Converts an object to a specified type, supporting nullable types, enums, and primitive conversions.
    /// If conversion fails, it returns the default value of the target type.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="o">The object to convert. If null, the default value of <typeparamref name="T"/> is returned.</param>
    /// <returns>The converted object of type <typeparamref name="T"/>, or default if conversion fails.</returns>
    [StswInfo(null)] 
    public static T? ConvertTo<T>(this object o) => o.ConvertTo(typeof(T)) is T tResult ? tResult : default;

    /// <summary>
    /// Infers the corresponding <see cref="SqlDbType"/> for a given .NET <see cref="Type"/>.
    /// Defaults to <see cref="SqlDbType.NVarChar"/> if no matching type is found.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    /// <returns>The corresponding <see cref="SqlDbType"/>, or <see langword="null"/> if no matching type is found.</returns>

    [StswInfo("0.9.0")]
    public static SqlDbType InferSqlDbType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        if (underlyingType.IsEnum)
            underlyingType = Enum.GetUnderlyingType(underlyingType);

        return TypeToSqlDbTypeMap.TryGetValue(underlyingType) ?? SqlDbType.NVarChar;
    }
    private static readonly Dictionary<Type, SqlDbType?> TypeToSqlDbTypeMap = new()
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
        { typeof(TimeSpan), SqlDbType.Time },
        { typeof(byte[]), SqlDbType.VarBinary },
        //{ typeof(object), SqlDbType.Variant }
    };

    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"> The type of objects to map to.</typeparam>
    /// <param name="dt"> The <see cref="DataTable"/> to map.</param>
    /// <returns>One or more objects of type <typeparamref name="T"/> mapped from the <see cref="DataTable"/>.</returns>

    [StswInfo("0.2.0")]
    public static IEnumerable<T?> MapTo<T>(this DataTable dt)
    {
        var type = typeof(T);

        if (IsSimpleType(type))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo<T?>();
        }
        else
        {
            foreach (var obj in StswMapping.MapToClass(dt, type))
                yield return (T?)obj;
        }
    }

    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of objects of a specified type.
    /// </summary>
    /// <param name="dt"> The <see cref="DataTable"/> to map.</param>
    /// <param name="type"> The type of objects to map to.</param>
    /// <returns>One or more objects of the specified type mapped from the <see cref="DataTable"/>.</returns>

    [StswInfo("0.18.0")]
    public static IEnumerable<object?> MapTo(this DataTable dt, Type type)
    {
        if (IsSimpleType(type))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo(type);
        }
        else
        {
            foreach (var obj in StswMapping.MapToClass(dt, type))
                yield return obj;
        }
    }

    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of objects of type <typeparamref name="T"/>, supporting nested classes and custom delimiters.
    /// </summary>
    /// <typeparam name="T"> The type of objects to map to.</typeparam>
    /// <param name="dt"> The <see cref="DataTable"/> to map.</param>
    /// <param name="delimiter"> The delimiter used to separate nested properties in the column names.</param>
    /// <returns>One or more objects of type <typeparamref name="T"/> mapped from the <see cref="DataTable"/>.</returns>

    [StswInfo("0.9.0")]
    public static IEnumerable<T?> MapTo<T>(this DataTable dt, char delimiter)
    {
        var type = typeof(T);

        if (IsSimpleType(type))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo<T>();
        }
        else
        {
            foreach (var obj in StswMapping.MapToNestedClass(dt, type, delimiter))
                yield return (T?)obj;
        }
    }

    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of objects of a specified type, supporting nested classes and custom delimiters.
    /// </summary>
    /// <param name="dt"> The <see cref="DataTable"/> to map.</param>
    /// <param name="type"> The type of objects to map to.</param>
    /// <param name="delimiter"> The delimiter used to separate nested properties in the column names.</param>
    /// <returns>One or more objects of the specified type mapped from the <see cref="DataTable"/>.</returns>

    [StswInfo("0.18.0")]
    public static IEnumerable<object?> MapTo(this DataTable dt, Type type, char delimiter)
    {
        if (IsSimpleType(type))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo(type);
        }
        else
        {
            foreach (var obj in StswMapping.MapToNestedClass(dt, type, delimiter))
                yield return obj;
        }
    }

    /// <summary>
    /// Checks if the specified type is a simple type (primitive, string, or byte array).
    /// </summary>
    /// <param name="type"> The type to check.</param>
    /// <returns><see langword="true"/> if the type is a simple type, <see langword="false"/> otherwise.</returns>
    [StswInfo("0.12.0")]
    private static bool IsSimpleType(Type type) => !type.IsClass || type == typeof(string) || type == typeof(byte[]);

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

    [StswInfo("0.9.0")]
    public static DataTable ToDataTable<T>(this IEnumerable<T> data)
    {
        var dataTable = new DataTable(typeof(T).Name);
        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(p => p.MetadataToken)
            .ToArray();

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
    #endregion

    #region DateTime extensions
    /// <summary>
    /// Calculates the quarter of the year for a given <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The <see cref="DateTime"/> to calculate the quarter for.</param>
    /// <returns>The quarter of the year (1 to 4) that the date falls in.</returns>
    [StswInfo("0.19.0")]
    public static int GetQuarter(this DateTime date) => (date.Month - 1) / 3 + 1;

    /// <summary>
    /// Checks if two <see cref="DateTime"/> instances are in the same year and month.
    /// </summary>
    /// <param name="dt1">The first <see cref="DateTime"/> instance to compare.</param>
    /// <param name="dt2">The second <see cref="DateTime"/> instance to compare.</param>
    /// <returns><see langword="true"/> if both <see cref="DateTime"/> instances are in the same year and month; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.17.0")]
    public static bool IsSameYearAndMonth(this DateTime dt1, DateTime dt2) => dt1.Year == dt2.Year && dt1.Month == dt2.Month;

    /// <summary>
    /// Finds the next occurrence of a specified day of the week after a given date.
    /// </summary>
    /// <param name="date">The starting date.</param>
    /// <param name="dayOfWeek">The day of the week to find.</param>
    /// <returns>The next date that falls on the specified day of the week.</returns>
    /// <remarks>
    /// This method is useful for scheduling or finding specific days, such as the next Monday after a given date.
    /// </remarks>
    [StswInfo("0.10.0")]
    public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek)
    {
        var start = (int)date.DayOfWeek;
        var target = (int)dayOfWeek;

        var daysToAdd = (target - start + 7) % 7;
        if (daysToAdd == 0) daysToAdd = 7;

        return date.AddDays(daysToAdd);
    }

    /// <summary>
    /// Returns the first day of the month for the given <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The source <see cref="DateTime"/>.</param>
    /// <returns>A <see cref="DateTime"/> representing the first day of the month at 00:00:00.</returns>
    [StswInfo("0.15.0")]
    public static DateTime ToFirstDayOfMonth(this DateTime date) => new DateTime(date.Year, date.Month, 1);

    /// <summary>
    /// Returns the last day of the month for the given <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The source <see cref="DateTime"/>.</param>
    /// <returns>A <see cref="DateTime"/> representing the last day of the month at 00:00:00.</returns>
    [StswInfo("0.15.0")]
    public static DateTime ToLastDayOfMonth(this DateTime date) => new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

    /// <summary>
    /// Converts a <see cref="DateTime"/> to a Unix timestamp.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
    /// <returns>A long value representing the number of seconds since the Unix epoch (January 1, 1970).</returns>
    [StswInfo("0.9.0")]
    public static long ToUnixTimeSeconds(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    #endregion

    #region Dictionary extensions
    /// <summary>
    /// Changes the key of an existing entry in the dictionary.
    /// </summary>
    /// <typeparam name="TKey">Type of dictionary keys.</typeparam>
    /// <typeparam name="TValue">Type of dictionary values.</typeparam>
    /// <param name="dict">The dictionary where the key should be changed.</param>
    /// <param name="oldKey">The existing key to be replaced.</param>
    /// <param name="newKey">The new key to assign.</param>
    /// <param name="overwriteExisting">Indicates whether to overwrite the existing entry if the new key already exists.</param>
    /// <returns><see langword="true"/> if the key was successfully changed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if dictionary is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if newKey already exists and overwriteExisting is <see langword="false"/>.</exception>
    [StswInfo("0.15.0")]
    public static bool ChangeKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey oldKey, TKey newKey, bool overwriteExisting = false)
    {
        ArgumentNullException.ThrowIfNull(dict);

        if (EqualityComparer<TKey>.Default.Equals(oldKey, newKey))
            return false;

        if (dict.ContainsKey(newKey) && !overwriteExisting)
            throw new ArgumentException($"The key '{newKey}' already exists in the dictionary.", nameof(newKey));

        if (!dict.Remove(oldKey, out var value))
            return false;

        dict[newKey] = value;
        return true;
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
    [StswInfo("0.10.0")]
    public static Dictionary<TKey, TValue> ToDictionarySafely<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>();

        foreach (var item in source)
            dictionary.TryAdd(keySelector(item), valueSelector(item));

        return dictionary;
    }

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key from the dictionary.
    /// Returns the value if the key exists; otherwise, returns <see langword="null"/> (for reference types)
    /// or <see langword="default"/> wrapped in a nullable type (for value types).
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key, or <see langword="default"/> if the key is not found.</returns>
    public static TValue? TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull => dictionary.TryGetValue(key, out var value) ? value : default;
    #endregion

    #region Enum extensions
    /// <summary>
    /// Gets an attribute of a specified type on an enum field value.
    /// </summary>
    /// <typeparam name="T">The type of the attribute to retrieve.</typeparam>
    /// <param name="enumVal">The enum value.</param>
    /// <returns>The attribute of type <typeparamref name="T"/> that exists on the enum value, or <see langword="null"/> if no such attribute is found.</returns>
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
    [StswInfo("0.10.0")]
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
    [StswInfo("0.5.0")]
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

    #region List extensions
    /// <summary>
    /// Adds an item to a list if the list does not already contain the item.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to add the item to.</param>
    /// <param name="item">The item to add.</param>
    [StswInfo("0.9.0")]
    public static void AddIfNotContains<T>(this IList<T> list, T item)
    {
        if (!list.Contains(item))
            list.Add(item);
    }

    /// <summary>
    /// Adds multiple items to the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="list">The collection to add items to.</param>
    /// <param name="items">The items to add.</param>
    [StswInfo("0.15.0")]
    public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> items)
    {
        foreach (var item in items)
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
    [StswInfo("0.10.0", IsTested = false)]
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
    /// Performs the specified action on each element of the <see cref="IEnumerable{}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <see cref="IEnumerable{}"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{}"/> to iterate over.</param>
    /// <param name="action">The action to perform on each element.</param>
    /// <exception cref="ArgumentNullException">Thrown when source or action is <see langword="null"/>.</exception>
    [StswInfo("0.15.0")]
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
            action(item);
    }

    /// <summary>
    /// Removes all occurrences of the specified elements from the <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to remove elements from.</param>
    /// <param name="items">The collection containing the elements to remove.</param>
    [StswInfo("0.6.1")]
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
    /// Replaces all occurrences of a specified value in an <see cref="IList{T}"/> with another value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="oldValue">The value to replace.</param>
    /// <param name="newValue">The value to replace with.</param>
    /// <exception cref="ArgumentNullException">Thrown when the source collection is <see langword="null"/>.</exception>
    [StswInfo("0.9.0")]
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
    [StswInfo("0.10.0", IsTested = false)]
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
    [StswInfo(null)]
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
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is <see langword="null"/>.</exception>
    [StswInfo(null)]
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
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is <see langword="null"/>.</exception>
    [StswInfo(null)]
    public static bool In<T>(this T value, params T[] parameters) => parameters == null
            ? throw new ArgumentNullException(nameof(parameters))
            : parameters.Contains(value);

    /// <summary>
    /// Checks if a type is a list type and retrieves the inner type if it is. In this method, <see cref="string"/> is not considered a list type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="innerType">The inner type if the type is a list type.</param>
    /// <returns><see langword="true"/> if the type is a list type; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.6.0")]
    public static bool IsListType(this Type type, out Type? innerType)
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
    /// Checks if a nullable value is either <see langword="null"/>, the default value for its type, or an empty collection.
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns><see langword="true"/> if the value is <see langword="null"/>, the default value for its type, or an empty collection; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool IsNullOrDefault(this DateTime? value) => value == null || value == default;

    /// <summary>
    /// Checks if a nullable value is either <see langword="null"/>, the default value for its type, or an empty collection.
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns><see langword="true"/> if the value is <see langword="null"/>, the default value for its type, or an empty collection; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool IsNullOrDefault(this DateTimeOffset? value) => value == null || value == default;

    /// <summary>
    /// Checks if a nullable value is either <see langword="null"/>, the default value for its type, or an empty collection.
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns><see langword="true"/> if the value is <see langword="null"/>, the default value for its type, or an empty collection; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool IsNullOrDefault(this decimal? value) => value == null || value == default;

    /// <summary>
    /// Checks if a nullable value is either <see langword="null"/>, the default value for its type, or an empty collection.
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns><see langword="true"/> if the value is <see langword="null"/>, the default value for its type, or an empty collection; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool IsNullOrDefault(this double? value) => value == null || value == default;

    /// <summary>
    /// Checks if a nullable value is either <see langword="null"/>, the default value for its type, or an empty collection.
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns><see langword="true"/> if the value is <see langword="null"/>, the default value for its type, or an empty collection; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool IsNullOrDefault<T>(this T? value) where T : Enum => value == null || EqualityComparer<T>.Default.Equals(value, default);

    /// <summary>
    /// Checks if a nullable value is either <see langword="null"/>, the default value for its type, or an empty collection.
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns><see langword="true"/> if the value is <see langword="null"/>, the default value for its type, or an empty collection; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool IsNullOrDefault(this float? value) => value == null || value == default;

    /// <summary>
    /// Checks if a nullable value is either <see langword="null"/>, the default value for its type, or an empty collection.
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns><see langword="true"/> if the value is <see langword="null"/>, the default value for its type, or an empty collection; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool IsNullOrDefault(this Guid? value) => value == null || value == Guid.Empty;

    /// <summary>
    /// Checks if a nullable value is either <see langword="null"/>, the default value for its type, or an empty collection.
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns><see langword="true"/> if the value is <see langword="null"/>, the default value for its type, or an empty collection; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool IsNullOrDefault(this int? value) => value == null || value == default;

    /// <summary>
    /// Checks if a nullable value is either <see langword="null"/>, the default value for its type, or an empty collection.
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns><see langword="true"/> if the value is <see langword="null"/>, the default value for its type, or an empty collection; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool IsNullOrDefault(this TimeSpan? value) => value == null || value == default;

    /// <summary>
    /// Checks if the given enumerable is null or empty.
    /// </summary>
    /// <param name="source">The enumerable to check.</param>
    /// <returns><see langword="true"/> if the enumerable is null or empty; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.15.0")]
    public static bool IsNullOrEmpty(this IEnumerable? source) => source == null || !source.GetEnumerator().MoveNext();

    /// <summary>
    /// Checks if the given generic enumerable is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable to check.</param>
    /// <returns><see langword="true"/> if the enumerable is null or empty; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.15.0")]
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => source == null || !source.Any();

    /// <summary>
    /// Determines whether a type is a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is numeric; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.3.0")]
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
    /// <returns><see langword="true"/> if both objects are either null or have identical public property values; otherwise, <see langword="false"/>.</returns>
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when max is less than or equal to 0.</exception>
    [StswInfo("0.6.0")]
    public static int ShiftBy(this int value, int step, int max, bool isLoopingAllowed = true)
    {
        if (max <= 0)
            throw new ArgumentOutOfRangeException(nameof(max), "Max must be greater than zero.");

        var newValue = value + step;

        if (isLoopingAllowed)
        {
            newValue %= max;
            if (newValue < 0)
                newValue += max;
        }
        else
        {
            newValue = Math.Clamp(newValue, 0, max - 1);
        }

        return newValue;
    }
    #endregion

    #region Object extensions
    /// <summary>
    /// Copies readable public properties from a source object to writable public properties 
    /// of a target object, matching by property name (case-insensitive).
    /// Supports conversion between compatible types, including nullable types and enums.
    /// Properties that do not exist on either side or cannot be assigned are ignored.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target object (destination).</typeparam>
    /// <typeparam name="TSource">The type of the source object (data provider).</typeparam>
    /// <param name="target">The target object to which values will be copied.</param>
    /// <param name="source">The source object from which values will be read.</param>
    [StswInfo("0.18.0")]
    public static void CopyFrom<TTarget, TSource>(this TTarget target, TSource source) where TTarget : class
    {
        if (target == null || source == null)
            return;

        var targetProps = typeof(TTarget)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        var sourceProps = typeof(TSource)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var targetProp in targetProps)
        {
            if (sourceProps.TryGetValue(targetProp.Name, out var sourceProp))
            {
                var sourceValue = sourceProp.GetValue(source);

                if (targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                    targetProp.SetValue(target, sourceValue);
                else
                    targetProp.SetValue(target, sourceValue.ConvertTo(targetProp.PropertyType));
            }
        }
    }

    /// <summary>
    /// Creates a deep copy of the object using JSON serialization.
    /// Works with serializable objects, including simple classes and collections.
    /// </summary>
    /// <typeparam name="T">Type of the object to copy.</typeparam>
    /// <param name="original">The object to clone.</param>
    /// <returns>A new instance with the same data, or <see langword="null"/> if cloning fails.</returns>
    [StswInfo("0.13.0")]
    public static T? DeepCopyWithJson<T>(this T original) where T : class
    {
        if (original == null)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(original));
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Recursively compares two objects of the same type to determine if all public properties and their nested values are equal.
    /// </summary>
    /// <typeparam name="T">The type of the objects being compared.</typeparam>
    /// <param name="obj1">The first object to compare.</param>
    /// <param name="obj2">The second object to compare.</param>
    /// <returns><see langword="true"/> if all public properties (including nested properties and collections) are equal; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.18.0")]
    public static bool DeepEquals<T>(this T obj1, T obj2) where T : class => DeepEquals(obj1, obj2, []);

    /// <summary>
    /// Recursively compares two objects of the same type using reflection. 
    /// Supports deep comparison of nested objects and collections. Cycles are handled using a visited hash set.
    /// </summary>
    /// <param name="obj1">The first object to compare.</param>
    /// <param name="obj2">The second object to compare.</param>
    /// <param name="visited">A set of already visited objects to avoid circular reference loops.</param>
    /// <returns><see langword="true"/> if the objects and all their properties are equal; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.18.0")]
    private static bool DeepEquals(object? obj1, object? obj2, HashSet<object> visited)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;

        if (obj1 == null || obj2 == null)
            return false;

        if (visited.Contains(obj1) || visited.Contains(obj2))
            return true;

        visited.Add(obj1);
        visited.Add(obj2);

        var type = obj1.GetType();

        if (type != obj2.GetType())
            return false;

        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime))
            return obj1.Equals(obj2);

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            var enum1 = ((IEnumerable)obj1).Cast<object>().ToList();
            var enum2 = ((IEnumerable)obj2).Cast<object>().ToList();

            if (enum1.Count != enum2.Count)
                return false;

            for (var i = 0; i < enum1.Count; i++)
                if (!DeepEquals(enum1[i], enum2[i], visited))
                    return false;

            return true;
        }

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var val1 = prop.GetValue(obj1);
            var val2 = prop.GetValue(obj2);

            if (!DeepEquals(val1, val2, visited))
                return false;
        }

        return true;
    }
    #endregion

    #region Task extensions
    /// <summary>
    /// Executes the task and suppresses any exception.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [StswInfo("0.19.0")]
    public static async Task Try(this Task task)
    {
        try
        {
            await task;
        }
        catch
        {
            // Ignored
        }
    }

    /// <summary>
    /// Executes the task and suppresses any exception, returning default value.
    /// </summary>
    /// <typeparam name="T">The type of the result of the task.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <returns>A task that represents the asynchronous operation, returning the result of the task or <see langword="default"/> if an exception occurs.</returns>
    [StswInfo("0.19.0")]
    public static async Task<T?> Try<T>(this Task<T> task)
    {
        try
        {
            return await task;
        }
        catch
        {
            return default;
        }
    }
    #endregion

    #region Text extensions
    /// <summary>
    /// Capitalizes the first letter of the string and converts the rest of the characters to lowercase.
    /// </summary>
    /// <param name="text">The string to capitalize.</param>
    /// <returns>A string with the first letter capitalized and the rest in lowercase.</returns>
    [StswInfo(null)]
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
    [StswInfo("0.1.0")]
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
    [StswInfo("0.1.0")]
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
    [StswInfo(null)]
    public static string TrimEnd(this string source, string value)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            return source;

        return source.EndsWith(value) ? source[..source.LastIndexOf(value)] : source;
    }

    /// <summary>
    /// Removes the specified string from the start of the current string instance.
    /// </summary>
    /// <param name="source">The string to trim.</param>
    /// <param name="value">The string to remove from the start.</param>
    /// <returns>A new string that is equivalent to the original string but without the specified value at the start.</returns>
    [StswInfo(null)]
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
    [StswInfo("0.9.0")]
    public static object? GetPropertyValue(this object obj, string propertyName) => propertyName == null ? null : obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
    #endregion
}
