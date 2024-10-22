using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System;

namespace StswExpress;

/// <summary>
/// Helper class for mapping <see cref="DataTable"/> rows to objects, including nested properties.
/// </summary>
internal static class StswMapping
{
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
             || HasNestedPropertiesInColumns(prop.PropertyType, normalizedColumnNames, delimiter, propPath))
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
    internal static bool HasNestedPropertiesInColumns(Type type, string[] normalizedColumnNames, char delimiter, string parentPath)
    {
        if (type.IsClass && type != typeof(string))
            return false;

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            var propPath = $"{parentPath}{delimiter}{prop.Name}";
            if (normalizedColumnNames.Any(col => col.Equals(propPath, StringComparison.CurrentCultureIgnoreCase)))
                return true;
        }
        
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
            var prop = kvp.Value;
            var columnName = kvp.Key;

            var columnIndex = Array.FindIndex(normalizedColumnNames, col => col.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
            if (columnIndex >= 0)
            {
                var value = row[columnIndex] == DBNull.Value ? null : row[columnIndex];
                SetNestedPropertyValue(obj, propPath, 0, value, propCache);
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
    internal static void SetNestedPropertyValue(object? obj, string[] propPath, int level, object? value, Dictionary<string, PropertyInfo> propCache)
    {
        if (level == propPath.Length - 1)
        {
            var finalProp = obj?.GetType().GetProperty(propPath[level], BindingFlags.Public | BindingFlags.Instance);
            if (finalProp != null)
            {
                var convertedValue = value?.ConvertTo(finalProp.PropertyType);
                finalProp.SetValue(obj, convertedValue);
            }
        }
        else
        {
            var propName = propPath[level];
            var prop = obj?.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                var nestedObj = prop.GetValue(obj);
                if (nestedObj == null)
                {
                    nestedObj = Activator.CreateInstance(prop.PropertyType);
                    prop.SetValue(obj, nestedObj);
                }
                SetNestedPropertyValue(nestedObj, propPath, level + 1, value, propCache);
            }
        }
    }
}
