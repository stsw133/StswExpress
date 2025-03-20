using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StswExpress.Commons;
/// <summary>
/// Collection of extension methods for various types and objects. These methods simplify common tasks and provide additional functionality beyond what is available in the standard WPF API.
/// </summary>
public static partial class StswExtensions
{
    #region Clone extensions
    /// <summary>
    /// Creates a deep copy of the specified object using JSON serialization.
    /// Suitable for serializable objects, including primitives, collections, and simple POCOs.
    /// Ignores properties marked with <see cref="JsonIgnoreAttribute"/>.
    /// Does not handle cyclic references.
    /// </summary>
    /// <typeparam name="T">The type of the object being cloned.</typeparam>
    /// <param name="original">The original object to clone.</param>
    /// <returns>A deep copy of the original object. The returned object and any sub-objects are entirely independent of the original.</returns>
    /// <remarks>
    /// This method uses <see cref="JsonSerializer"/> for deep cloning by serializing the object to a JSON string and then deserializing it back to a new instance.
    /// It is suitable for objects that are serializable to JSON, including primitive types, complex objects, and collections. 
    /// However, types like <see cref="BitmapImage"/> and other WPF-related types cannot be serialized directly. These properties must be manually excluded using the <see cref="JsonIgnoreAttribute"/> or handled with custom converters.
    /// Circular references and metadata are not handled automatically and may cause issues if present. Use with caution for objects with complex internal state or cyclic dependencies.
    /// </remarks>
    public static T? DeepCopyWithJson<T>(this T original) where T : class
    {
        if (original == null)
            return default;

        try
        {
            var jsonString = JsonSerializer.Serialize(original);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch (JsonException)
        {
            return default;
        }
    }
    #endregion

    #region Convert extensions
    /// <summary>
    /// Converts an object to a specified type, supporting nullable types, enums, and primitive conversions.
    /// If conversion fails, it returns the default value of the target type.
    /// </summary>
    /// <param name="o">The object to convert. If <see langword="null"/>, the <see langword="null"/> is returned.</param>
    /// <param name="t">The target type to convert to.</param>
    /// <returns>The converted object of type <paramref name="t"/>, or <see langword="null"/> if conversion fails.</returns>
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
    public static T? ConvertTo<T>(this object o) => o.ConvertTo(typeof(T)) is T tResult ? tResult : default;

    /// <summary>
    /// Infers the corresponding <see cref="SqlDbType"/> for a given .NET <see cref="Type"/>.
    /// Defaults to <see cref="SqlDbType.NVarChar"/> if no matching type is found.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    /// <returns>The corresponding <see cref="SqlDbType"/>, or <see langword="null"/> if no matching type is found.</returns>
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
    /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="StswObservableCollection{T}"/>.
    /// Supports tracking removed items and ignoring specified property changes.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="value">The enumerable to convert.</param>
    /// <returns>The converted <see cref="StswObservableCollection{T}"/>.</returns>
    public static StswObservableCollection<T> ToStswObservableCollection<T>(this IEnumerable<T> value, bool showRemovedItems = false, IEnumerable<string>? ignoredPropertyNames = null) where T : IStswCollectionItem => new(value, showRemovedItems, ignoredPropertyNames);

    /// <summary>
    /// Converts an <see cref="IDictionary{TKey, TValue}"/> to a <see cref="StswObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="value">The dictionary to convert.</param>
    /// <returns>The converted <see cref="StswObservableDictionary{TKey, TValue}"/>.</returns>
    public static StswObservableDictionary<TKey, TValue> ToStswObservableDictionary<TKey, TValue>(this IDictionary<TKey, TValue> value, bool autoAddOnGet = true) where TKey : notnull => new(value) { AutoAddOnGet = autoAddOnGet };
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
    /// Returns the first day of the month for the given <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The source <see cref="DateTime"/>.</param>
    /// <returns>A <see cref="DateTime"/> representing the first day of the month at 00:00:00.</returns>
    public static DateTime ToFirstDayOfMonth(this DateTime date) => new DateTime(date.Year, date.Month, 1);

    /// <summary>
    /// Returns the last day of the month for the given <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The source <see cref="DateTime"/>.</param>
    /// <returns>A <see cref="DateTime"/> representing the last day of the month at 00:00:00.</returns>
    public static DateTime ToLastDayOfMonth(this DateTime date) => new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

    /// <summary>
    /// Converts a <see cref="DateTime"/> to a Unix timestamp.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
    /// <returns>A long value representing the number of seconds since the Unix epoch (January 1, 1970).</returns>
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
    /// <param name="overwriteExisting">
    /// If true, replaces the value of an existing newKey. 
    /// If false, throws an exception if newKey already exists.
    /// </param>
    /// <returns>True if the key was successfully changed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if dictionary is null.</exception>
    /// <exception cref="ArgumentException">Thrown if newKey already exists and overwriteExisting is false.</exception>
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
    /// Adds multiple items to the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="list">The collection to add items to.</param>
    /// <param name="items">The items to add.</param>
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
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
            action(item);
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
    /// <exception cref="ArgumentNullException">Thrown when the source collection is <see langword="null"/>.</exception>
    public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T oldValue, T newValue)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Select(item => EqualityComparer<T>.Default.Equals(item, oldValue) ? newValue : item);
    }

    /// <summary>
    /// Replaces all occurrences of a specified value in an <see cref="IList{T}"/> with another value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="oldValue">The value to replace.</param>
    /// <param name="newValue">The value to replace with.</param>
    /// <exception cref="ArgumentNullException">Thrown when the source collection is <see langword="null"/>.</exception>
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
    /// Checks if a value is null, the default for its type, or an empty collection.
    /// Supports reference types, value types, and collections.
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

        if (type.IsEnum)
            return EqualityComparer<T>.Default.Equals(value, default);

        if (value is IEnumerable enumerable)
            return enumerable.IsNullOrEmpty();

        if (type.IsClass)
            return value.IsSimilarTo((T?)Activator.CreateInstance(type));

        return EqualityComparer<T>.Default.Equals(value, default);
    }

    /// <summary>
    /// Checks if the given enumerable is null or empty.
    /// </summary>
    /// <param name="source">The enumerable to check.</param>
    /// <returns><see langword="true"/> if the enumerable is null or empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty(this IEnumerable source) => source == null || !source.GetEnumerator().MoveNext();

    /// <summary>
    /// Checks if the given generic enumerable is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable to check.</param>
    /// <returns><see langword="true"/> if the enumerable is null or empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => source == null || !source.Any();

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
}
