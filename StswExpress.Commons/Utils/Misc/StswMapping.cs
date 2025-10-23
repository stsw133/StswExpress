using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace StswExpress.Commons;

/// <summary>
/// Helper class for mapping <see cref="DataTable"/> rows to objects, including nested properties.
/// </summary>
public static class StswMapping
{
    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"> The type of objects to map to.</typeparam>
    /// <param name="dt"> The <see cref="DataTable"/> to map.</param>
    /// <returns>One or more objects of type <typeparamref name="T"/> mapped from the <see cref="DataTable"/>.</returns>
    public static IEnumerable<T> MapTo<T>(this DataTable dt)
    {
        var type = typeof(T);

        if (IsSimpleType(type))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo<T>()!;
        }
        else if (IsKeyValuePairType(type))
        {
            foreach (var obj in MapToKeyValuePair(dt, type))
                yield return (T)obj;
        }
        else if (IsTupleType(type))
        {
            foreach (var obj in MapToTuple(dt, type))
                yield return (T)obj;
        }
        else
        {
            foreach (var obj in MapToClass(dt, type))
                yield return (T)obj;
        }
    }

    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of objects of a specified type.
    /// </summary>
    /// <param name="dt"> The <see cref="DataTable"/> to map.</param>
    /// <param name="type"> The type of objects to map to.</param>
    /// <returns>One or more objects of the specified type mapped from the <see cref="DataTable"/>.</returns>
    public static IEnumerable<object> MapTo(this DataTable dt, Type type)
    {
        if (IsSimpleType(type))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo(type)!;
        }
        else if (IsKeyValuePairType(type))
        {
            foreach (var obj in MapToKeyValuePair(dt, type))
                yield return obj;
        }
        else if (IsTupleType(type))
        {
            foreach (var obj in MapToTuple(dt, type))
                yield return obj;
        }
        else
        {
            foreach (var obj in MapToClass(dt, type))
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
    public static IEnumerable<T> MapTo<T>(this DataTable dt, char delimiter)
    {
        var type = typeof(T);

        if (IsSimpleType(type))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo<T>()!;
        }
        else if (IsKeyValuePairType(type))
        {
            foreach (var obj in MapToKeyValuePair(dt, type))
                yield return (T)obj;
        }
        else if (IsTupleType(type))
        {
            foreach (var obj in MapToTuple(dt, type))
                yield return (T)obj;
        }
        else
        {
            foreach (var obj in MapToNestedClass(dt, type, delimiter))
                yield return (T)obj;
        }
    }

    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of objects of a specified type, supporting nested classes and custom delimiters.
    /// </summary>
    /// <param name="dt"> The <see cref="DataTable"/> to map.</param>
    /// <param name="type"> The type of objects to map to.</param>
    /// <param name="delimiter"> The delimiter used to separate nested properties in the column names.</param>
    /// <returns>One or more objects of the specified type mapped from the <see cref="DataTable"/>.</returns>
    public static IEnumerable<object> MapTo(this DataTable dt, Type type, char delimiter)
    {
        if (IsSimpleType(type))
        {
            foreach (var value in dt.AsEnumerable().Select(x => x[0]))
                yield return value.ConvertTo(type)!;
        }
        else if (IsKeyValuePairType(type))
        {
            foreach (var obj in MapToKeyValuePair(dt, type))
                yield return obj;
        }
        else if (IsTupleType(type))
        {
            foreach (var obj in MapToTuple(dt, type))
                yield return obj;
        }
        else
        {
            foreach (var obj in MapToNestedClass(dt, type, delimiter))
                yield return obj;
        }
    }

    #region MapTo components
    /// <summary>
    /// Checks if a type is a simple type, which includes primitive types, strings, decimals, DateTime, DateTimeOffset, TimeSpan, Guids, and byte arrays.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a simple type, <see langword="false"/> otherwise.</returns>
    public static bool IsSimpleType(Type type)
    {
        if (type.IsEnum) return true;
        if (Nullable.GetUnderlyingType(type) is Type u) type = u;

        return type.IsPrimitive
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid)
            || type == typeof(byte[]);
    }

    /// <summary>
    /// Checks if a type is a KeyValuePair type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a KeyValuePair type, <see langword="false"/> otherwise.</returns>
    private static bool IsKeyValuePairType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);

    /// <summary>
    /// Checks if a type is a Tuple or ValueTuple type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a Tuple or ValueTuple type, <see langword="false"/> otherwise.</returns>
    private static bool IsTupleType(Type type)
        => type.FullName?.StartsWith("System.ValueTuple", StringComparison.Ordinal) == true
            || type.FullName?.StartsWith("System.Tuple", StringComparison.Ordinal) == true;

    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of KeyValuePair objects of a specified type.
    /// </summary>
    /// <param name="dt">The <see cref="DataTable"/> to map.</param>
    /// <param name="keyValuePairType">The type of KeyValuePair to map to.</param>
    /// <returns>One or more KeyValuePair objects of the specified type mapped from the <see cref="DataTable"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="DataTable"/> does not contain enough columns to map to a KeyValuePair.</exception>
    private static IEnumerable<object> MapToKeyValuePair(DataTable dt, Type keyValuePairType)
    {
        var genericArgs = keyValuePairType.GetGenericArguments();
        if (genericArgs.Length != 2)
            yield break;

        var columnLookup = dt.Columns.Cast<DataColumn>()
            .Select((column, index) => new { column.ColumnName, index })
            .ToDictionary(x => x.ColumnName, x => x.index, StringComparer.OrdinalIgnoreCase);

        var keyIndex = columnLookup.TryGetValue("Key", out var idx) ? idx : -1;
        var valueIndex = columnLookup.TryGetValue("Value", out idx) ? idx : -1;

        if (keyIndex < 0 || valueIndex < 0)
        {
            if (dt.Columns.Count < 2)
                throw new InvalidOperationException("Cannot map to KeyValuePair because the result does not contain enough columns.");

            keyIndex = 0;
            valueIndex = 1;
        }

        foreach (DataRow row in dt.Rows)
        {
            var key = row[keyIndex].ConvertTo(genericArgs[0]);
            var value = row[valueIndex].ConvertTo(genericArgs[1]);
            yield return Activator.CreateInstance(keyValuePairType, key, value)!;
        }
    }

    /// <summary>
    /// Maps a <see cref="DataTable"/> to a collection of Tuple objects of a specified type.
    /// </summary>
    /// <param name="dt">The <see cref="DataTable"/> to map.</param>
    /// <param name="tupleType">The type of Tuple to map to.</param>
    /// <returns>One or more Tuple objects of the specified type mapped from the <see cref="DataTable"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="DataTable"/> does not contain enough columns to map to the Tuple.</exception>
    private static IEnumerable<object> MapToTuple(DataTable dt, Type tupleType)
    {
        var elementTypes = GetTupleElementTypes(tupleType);
        if (elementTypes.Count == 0)
            yield break;

        if (dt.Columns.Count < elementTypes.Count)
            throw new InvalidOperationException($"Cannot map to tuple type '{tupleType.FullName}' because the result does not contain enough columns.");

        var columnIndices = GetTupleColumnIndices(dt, elementTypes.Count);

        foreach (DataRow row in dt.Rows)
        {
            var values = new object?[elementTypes.Count];
            for (var i = 0; i < elementTypes.Count; i++)
                values[i] = row[columnIndices[i]].ConvertTo(elementTypes[i]);

            var index = 0;
            yield return BuildTuple(tupleType, values, ref index);
        }
    }

    /// <summary>
    /// Gets the element types of a Tuple or ValueTuple type.
    /// </summary>
    /// <param name="tupleType">The tuple type to analyze.</param>
    /// <returns>A list of element types in the tuple.</returns>
    private static List<Type> GetTupleElementTypes(Type tupleType)
    {
        var result = new List<Type>();
        CollectTupleElementTypes(tupleType, result);
        return result;
    }

    /// <summary>
    /// Recursively collects the element types of a Tuple or ValueTuple type.
    /// </summary>
    /// <param name="tupleType">The tuple type to analyze.</param>
    /// <param name="result">The list to collect the element types into.</param>
    private static void CollectTupleElementTypes(Type tupleType, List<Type> result)
    {
        if (!IsTupleType(tupleType) || !tupleType.IsGenericType)
            return;

        var genericArgs = tupleType.GetGenericArguments();
        for (var i = 0; i < genericArgs.Length; i++)
        {
            var arg = genericArgs[i];
            if (IsTupleType(arg))
                CollectTupleElementTypes(arg, result);
            else
                result.Add(arg);
        }
    }

    /// <summary>
    /// Gets the column indices in a <see cref="DataTable"/> that correspond to the elements of a Tuple type.
    /// </summary>
    /// <param name="dt">The <see cref="DataTable"/> to analyze.</param>
    /// <param name="elementCount">The number of elements in the tuple.</param>
    /// <returns>An array of integers representing the column indices for each tuple element.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="DataTable"/> does not contain enough columns to map to the tuple.</exception>
    private static int[] GetTupleColumnIndices(DataTable dt, int elementCount)
    {
        var indices = new int[elementCount];
        var columnLookup = dt.Columns.Cast<DataColumn>()
            .Select((column, index) => new { column.ColumnName, index })
            .ToDictionary(x => x.ColumnName, x => x.index, StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < elementCount; i++)
        {
            var itemName = $"Item{i + 1}";
            if (columnLookup.TryGetValue(itemName, out var index))
                indices[i] = index;
            else if (i < dt.Columns.Count)
                indices[i] = i;
            else
                throw new InvalidOperationException($"Cannot map to tuple because the result does not contain enough columns (expected {elementCount}).");
        }

        return indices;
    }

    /// <summary>
    /// Builds a Tuple object of a specified type from an array of values.
    /// </summary>
    /// <param name="tupleType">The type of Tuple to build.</param>
    /// <param name="values">An array of values to populate the Tuple.</param>
    /// <param name="index">A reference to the current index in the values array.</param>
    /// <returns>The constructed Tuple object.</returns>
    private static object BuildTuple(Type tupleType, object?[] values, ref int index)
    {
        var genericArgs = tupleType.GetGenericArguments();
        var ctorArgs = new object?[genericArgs.Length];

        for (var i = 0; i < genericArgs.Length; i++)
        {
            var argType = genericArgs[i];
            if (IsTupleType(argType))
                ctorArgs[i] = BuildTuple(argType, values, ref index);
            else
                ctorArgs[i] = values[index++];
        }

        return Activator.CreateInstance(tupleType, ctorArgs)!;
    }
    #endregion

    #region Nested class mapping components
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
    /// <returns>A dictionary mapping property paths to their corresponding <see cref="PropertyInfo"/>.</returns>
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
    /// <returns><see langword="true""/> if the type has nested properties that match any of the column names; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="propCache">A cache of property information for the object, used to optimize property access.</param>
    /// <param name="delimiter">The delimiter used to separate nested property names in the column names.</param>
    /// <returns>A list of <see cref="PropColumnMapping"/> that define how to map the columns to the object's properties.</returns>
    private static List<PropColumnMapping> PrepareColumnMappings(string[] normalizedNames, Dictionary<string, PropertyInfo> propCache, char delimiter)
    {
        var mappings = new List<PropColumnMapping>();
        for (var i = 0; i < normalizedNames.Length; i++)
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
    internal static IEnumerable<object> MapToClass(DataTable dt, Type type)
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
    internal static IEnumerable<object> MapToNestedClass(DataTable dt, Type type, char delimiter)
    {
        var normalizedColumnNames = dt.Columns.Cast<DataColumn>()
            .Select(x => StswFn.NormalizeDiacritics(x.ColumnName.Replace(" ", "")))
            .ToArray();

        var nameSet = new HashSet<string>(normalizedColumnNames, StringComparer.OrdinalIgnoreCase);
        var propCache = CacheProperties(type, nameSet, delimiter);
        var columnMappings = PrepareColumnMappings(normalizedColumnNames, propCache, delimiter);
        var factory = CreateInstanceFactory(type);

        foreach (var row in dt.AsEnumerable())
        {
            var obj = factory();
            if (obj is null) continue;

            MapRowToObject(obj, row, columnMappings, propCache, delimiter);
            yield return obj;
        }
    }
    #endregion
}
