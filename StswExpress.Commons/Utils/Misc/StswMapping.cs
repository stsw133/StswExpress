using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace StswExpress.Commons;

/// <summary>
/// Helper class for mapping <see cref="DataTable"/> rows to objects, including nested properties.
/// </summary>
[StswInfo("0.9.0")]
internal static class StswMapping
{
    /// <summary>
    /// A cache for instance factories to create objects of specific types.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, object> _instanceFactories = [];

    /// <summary>
    /// Creates a factory function to instantiate objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to create.</typeparam>
    /// <returns>A function that creates an instance of type <typeparamref name="T"/>.</returns>
    private static Func<T> CreateInstanceFactory<T>()
        => (Func<T>)_instanceFactories.GetOrAdd(typeof(T), t =>
        {
            var constructor = Expression.New(typeof(T));
            var lambda = Expression.Lambda<Func<T>>(constructor);
            return lambda.Compile();
        });

    /// <summary>
    /// Creates a factory function to instantiate objects of a specified type.
    /// </summary>
    /// <param name="type">The type of the object to create.</param>
    /// <returns>A function that creates an instance of the specified type.</returns>
    private static Func<object> CreateInstanceFactory(Type type)
        => (Func<object>)_instanceFactories.GetOrAdd(type, t =>
        {
            var constructor = Expression.New(t);
            var lambda = Expression.Lambda<Func<object>>(Expression.Convert(constructor, typeof(object)));
            return lambda.Compile();
        });

    /// <summary>
    /// Represents a mapping between a column in a <see cref="DataTable"/> and a property in an object.
    /// </summary>
    /// <param name="ColumnIndex">The index of the column in the <see cref="DataTable"/>.</param>
    /// <param name="PropPath">The path to the property in the object, represented as an array of strings.</param>
    /// <param name="FullPath">The full path to the property, including any nested properties, represented as a string.</param>
    /// <param name="PropInfo">The <see cref="PropertyInfo"/> of the property in the object.</param>
    internal record PropColumnMapping(int ColumnIndex, string[] PropPath, string FullPath, PropertyInfo PropInfo);

    /// <summary>
    /// Caches the properties of a type that match the specified column names from a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="type">The type whose properties are to be cached.</param>
    /// <param name="columnNames">The normalized column names from the <see cref="DataTable"/>.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column names.</param>
    /// <param name="parentPath">The parent path for nested properties, used for recursive calls.</param>
    /// <param name="visitedTypes">A set of visited types to avoid infinite recursion in case of circular references.</param>
    /// <returns></returns>
    private static Dictionary<string, PropertyInfo> CacheProperties(Type type, HashSet<string> columnNames, char delimiter, string parentPath = "", HashSet<Type>? visitedTypes = null)
    {
        visitedTypes ??= [];
        if (visitedTypes.Contains(type))
            return [];

        visitedTypes.Add(type);
        var propMappings = new Dictionary<string, PropertyInfo>();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite).ToArray();

        foreach (var prop in props)
        {
            var propPath = string.IsNullOrEmpty(parentPath) ? prop.Name : $"{parentPath}{delimiter}{prop.Name}";
            if (columnNames.Contains(propPath))
                propMappings[propPath] = prop;

            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string) && prop.PropertyType != typeof(byte[]))
            {
                var nested = CacheProperties(prop.PropertyType, columnNames, delimiter, propPath, visitedTypes);
                foreach (var kv in nested)
                    propMappings[kv.Key] = kv.Value;

                if (nested.Count > 0 && !propMappings.ContainsKey(propPath))
                    propMappings[propPath] = prop;
            }
        }

        visitedTypes.Remove(type);
        return propMappings;
    }

    /// <summary>
    /// Checks if a type has nested properties that match any of the normalized column names in a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="type">The type to check for nested properties.</param>
    /// <param name="normalizedColumnNames">An array of normalized column names from the <see cref="DataTable"/>.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column names.</param>
    /// <param name="parentPath">The parent path for nested properties, used for recursive calls.</param>
    /// <param name="visitedTypes">A set of visited types to avoid infinite recursion in case of circular references.</param>
    /// <returns></returns>
    private static bool HasNestedPropertiesInColumns(Type type, string[] normalizedColumnNames, char delimiter, string parentPath, HashSet<Type>? visitedTypes = null)
    {
        if (!type.IsClass || type == typeof(string))
            return false;

        visitedTypes ??= [];
        if (visitedTypes.Contains(type))
            return false;

        visitedTypes.Add(type);

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            var propPath = $"{parentPath}{delimiter}{prop.Name}";
            if (normalizedColumnNames.Any(col => col.Equals(propPath, StringComparison.CurrentCultureIgnoreCase)))
                return true;

            if (HasNestedPropertiesInColumns(prop.PropertyType, normalizedColumnNames, delimiter, propPath, visitedTypes))
                return true;
        }

        visitedTypes.Remove(type);
        return false;
    }

    /// <summary>
    /// Maps a <see cref="DataRow"/> to an object, setting nested properties based on the column mappings.
    /// </summary>
    /// <param name="obj">The object to map the row to.</param>
    /// <param name="row">The <see cref="DataRow"/> to map.</param>
    /// <param name="columnMappings">A list of column mappings that define how to map the row to the object.</param>
    /// <param name="propCache">A cache of property information for the object, used to optimize property access.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column mappings.</param>
    private static void MapRowToObject(object? obj, DataRow row, List<PropColumnMapping> columnMappings, Dictionary<string, PropertyInfo> propCache, char delimiter)
    {
        foreach (var mapping in columnMappings)
        {
            var value = row[mapping.ColumnIndex] == DBNull.Value ? null : row[mapping.ColumnIndex];
            SetNestedPropertyValue(obj, mapping.PropPath, 0, value, propCache, delimiter);
        }
    }

    /// <summary>
    /// Prepares a list of column mappings from normalized names and a <see cref="DataTable"/>, mapping each column to its corresponding property in the object.
    /// </summary>
    /// <param name="normalizedNames">An array of normalized column names from the <see cref="DataTable"/>.</param>
    /// <param name="dt">The <see cref="DataTable"/> containing the data to map.</param>
    /// <param name="propCache">A cache of property information for the object, used to optimize property access.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column names.</param>
    /// <returns></returns>
    private static List<PropColumnMapping> PrepareColumnMappings(string[] normalizedNames, DataTable dt, Dictionary<string, PropertyInfo> propCache, char delimiter)
    {
        var mappings = new List<PropColumnMapping>();
        for (int i = 0; i < normalizedNames.Length; i++)
        {
            var fullPath = normalizedNames[i];
            if (propCache.TryGetValue(fullPath, out var prop))
            {
                var path = fullPath.Split(delimiter);
                mappings.Add(new PropColumnMapping(i, path, fullPath, prop));
            }
        }

        return mappings;
    }

    /// <summary>
    /// Sets the value of a nested property in an object based on a property path and a value.
    /// </summary>
    /// <param name="obj">The object whose property value is to be set.</param>
    /// <param name="propPath">An array of strings representing the path to the property, where each element is a property name.</param>
    /// <param name="level">The current level in the property path, used for recursion.</param>
    /// <param name="value">The value to set for the property.</param>
    /// <param name="propCache">A cache of property information for the object, used to optimize property access.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the property path.</param>
    private static void SetNestedPropertyValue(object? obj, string[] propPath, int level, object? value, Dictionary<string, PropertyInfo> propCache, char delimiter)
    {
        if (obj == null)
            return;

        var fullPath = string.Join(delimiter.ToString(), propPath.Take(level + 1));
        if (!propCache.TryGetValue(fullPath, out var prop))
            return;

        if (level == propPath.Length - 1)
        {
            var converted = value?.ConvertTo(prop.PropertyType);
            prop.SetValue(obj, converted);
        }
        else
        {
            var nestedObj = prop.GetValue(obj);
            if (nestedObj == null)
            {
                nestedObj = _instanceFactories.TryGetValue(prop.PropertyType, out var factory)
                    ? ((Func<object>)factory)()
                    : Activator.CreateInstance(prop.PropertyType);

                prop.SetValue(obj, nestedObj);
            }

            SetNestedPropertyValue(nestedObj, propPath, level + 1, value, propCache, delimiter);
        }
    }

    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of objects of a specified type.
    /// </summary>
    /// <param name="dt"> The <see cref="DataTable"/> to map.</param>
    /// <param name="type"> The type of objects to map to.</param>
    /// <returns>One or more objects of the specified type mapped from the <see cref="DataTable"/>.</returns>
    [StswInfo("0.18.0")]
    internal static IEnumerable<object?> MapToClass(DataTable dt, Type type)
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

        var factory = CreateInstanceFactory(type);

        foreach (var row in dt.AsEnumerable())
        {
            var obj = factory();
            if (obj is null) continue;

            foreach (var kvp in mappings)
            {
                try
                {
                    var value = row[kvp.Key].ConvertTo(kvp.Value.PropertyType);
                    kvp.Value.SetValue(obj, value);
                }
                catch
                {
                    // Optional logging
                }
            }

            yield return obj;
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
    internal static IEnumerable<object?> MapToNestedClass(DataTable dt, Type type, char delimiter)
    {
        var normalizedColumnNames = dt.Columns.Cast<DataColumn>()
            .Select(x => StswFn.NormalizeDiacritics(x.ColumnName.Replace(" ", "")))
            .ToArray();

        var nameSet = new HashSet<string>(normalizedColumnNames, StringComparer.OrdinalIgnoreCase);
        var propCache = CacheProperties(type, nameSet, delimiter);
        var columnMappings = PrepareColumnMappings(normalizedColumnNames, dt, propCache, delimiter);
        var factory = CreateInstanceFactory(type);

        foreach (var row in dt.AsEnumerable())
        {
            var obj = factory();
            if (obj is null) continue;

            MapRowToObject(obj, row, columnMappings, propCache, delimiter);
            yield return obj;
        }
    }
}
