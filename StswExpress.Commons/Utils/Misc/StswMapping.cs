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
    internal static Func<T> CreateInstanceFactory<T>()
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
    internal static Func<object> CreateInstanceFactory(Type type)
        => (Func<object>)_instanceFactories.GetOrAdd(type, t =>
        {
            var constructor = Expression.New(t);
            var lambda = Expression.Lambda<Func<object>>(Expression.Convert(constructor, typeof(object)));
            return lambda.Compile();
        });

    /// <summary>
    /// Caches the properties of a given type, including nested properties, if they exist in the column names.
    /// </summary>
    /// <param name="type">The type of the object to cache properties for.</param>
    /// <param name="normalizedColumnNames">The normalized column names from the <see cref="DataTable"/>.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column names.</param>
    /// <param name="parentPath">The parent path for nested properties.</param>
    /// <param name="visitedTypes">A set of visited types to avoid recursion issues.</param>
    /// <returns>A dictionary of cached properties and their paths.</returns>
    internal static Dictionary<string, PropertyInfo> CacheProperties(Type type, string[] normalizedColumnNames, char delimiter, string parentPath = "", HashSet<Type>? visitedTypes = null)
    {
        visitedTypes ??= [];
        if (visitedTypes.Contains(type)) return [];

        visitedTypes.Add(type);
        var propMappings = new Dictionary<string, PropertyInfo>();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite).ToArray();

        foreach (var prop in props)
        {
            var propPath = string.IsNullOrEmpty(parentPath) ? prop.Name : $"{parentPath}{delimiter}{prop.Name}";

            if (normalizedColumnNames.Any(x => x.Equals(propPath, StringComparison.CurrentCultureIgnoreCase))
             || HasNestedPropertiesInColumns(prop.PropertyType, normalizedColumnNames, delimiter, propPath, visitedTypes))
            {
                propMappings[propPath] = prop;

                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(byte[]) && prop.PropertyType != typeof(string) && !visitedTypes.Contains(prop.PropertyType))
                {
                    var nestedProps = CacheProperties(prop.PropertyType, normalizedColumnNames, delimiter, propPath, visitedTypes);
                    foreach (var nestedProp in nestedProps)
                        propMappings[nestedProp.Key] = nestedProp.Value;
                }
            }
        }

        visitedTypes.Remove(type);
        return propMappings;
    }

    /// <summary>
    /// Checks if a given type has nested properties that match any of the normalized column names.
    /// </summary>
    /// <param name="type">The type to check for nested properties.</param>
    /// <param name="normalizedColumnNames">The normalized column names from the <see cref="DataTable"/>.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column names.</param>
    /// <param name="parentPath">The parent path for nested properties.</param>
    /// <returns><see langword="true"/> if there are nested properties that match the column names, otherwise <see langword="false"/>.</returns>
    internal static bool HasNestedPropertiesInColumns(Type type, string[] normalizedColumnNames, char delimiter, string parentPath, HashSet<Type>? visitedTypes = null)
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
    /// Maps a <see cref="DataRow"/> to an object, including setting nested property values.
    /// </summary>
    /// <param name="obj">The object to map the <see cref="DataRow"/> values to.</param>
    /// <param name="row">The <see cref="DataRow"/> to map from.</param>
    /// <param name="normalizedColumnNames">The normalized column names from the <see cref="DataTable"/>.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column names.</param>
    /// <param name="propCache">A dictionary of cached properties and their paths.</param>
    internal static void MapRowToObject(object? obj, DataRow row, string[] normalizedColumnNames, char delimiter, Dictionary<string, PropertyInfo> propCache)
    {
        foreach (var kvp in propCache)
        {
            var propPath = kvp.Key.Split(delimiter);
            var columnIndex = Array.FindIndex(normalizedColumnNames, col => col.Equals(kvp.Key, StringComparison.CurrentCultureIgnoreCase));
            if (columnIndex >= 0)
            {
                var value = row[columnIndex] == DBNull.Value ? null : row[columnIndex];
                SetNestedPropertyValue(obj, propPath, 0, value, propCache, delimiter);
            }
        }
    }

    /// <summary>
    /// Sets the value of a nested property on an object.
    /// </summary>
    /// <param name="obj">The object to set the property value on.</param>
    /// <param name="propPath">The path of the property to set.</param>
    /// <param name="level">The current level in the property path.</param>
    /// <param name="value">The value to set on the property.</param>
    /// <param name="propCache">A dictionary of cached properties and their paths.</param>
    internal static void SetNestedPropertyValue(object? obj, string[] propPath, int level, object? value, Dictionary<string, PropertyInfo> propCache, char delimiter)
    {
        if (obj == null)
            return;

        var fullPath = string.Join(delimiter.ToString(), propPath.Take(level + 1));
        if (!propCache.TryGetValue(fullPath, out var prop))
            return;

        if (level == propPath.Length - 1)
        {
            if (prop.PropertyType == typeof(object))
            {
                prop.SetValue(obj, value);
            }
            else
            {
                var convertedValue = value?.ConvertTo(prop.PropertyType);
                prop.SetValue(obj, convertedValue);
            }
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
}
