using System.Reflection;
using System.Collections;

namespace StswExpress.Commons;

/// <summary>
/// Utility class for generating random instances of a specified type.
/// </summary>
[StswInfo("0.16.0")]
public static class StswRandomGenerator
{
    private static readonly Random _random = new();

    /// <summary>
    /// Generates a sequence of random items of type T with random property values.
    /// </summary>
    /// <typeparam name="T">The type of the model to generate.</typeparam>
    /// <param name="count">The number of items to generate.</param>
    /// <returns>IEnumerable of T with randomly populated properties.</returns>
    public static IEnumerable<T> CreateRandomItems<T>(int count)
    {
        var visited = new HashSet<Type>();

        for (int i = 0; i < count; i++)
            yield return (T)GenerateRandom(typeof(T), visited)!;
    }

    /// <summary>
    /// Generates a random value for the specified type.
    /// The method supports basic types (string, int, bool, enum, etc.), arrays,
    /// generic collections (List&lt;T&gt;, IList&lt;T&gt;, IEnumerable&lt;T&gt;, ICollection&lt;T&gt;)
    /// and complex objects (classes/structs). In case of nested objects, recursion is prevented
    /// when the type equals a parent type.
    /// </summary>
    /// <param name="type">The type for which to generate a random value.</param>
    /// <param name="visited">A set of types currently being processed (to prevent infinite recursion).</param>
    /// <returns>A random value for the given type or null in some cases (e.g. nullable or recursive reference).</returns>
    private static object? GenerateRandom(Type type, HashSet<Type> visited)
    {
        if (visited.Contains(type))
            return null;

        if (Nullable.GetUnderlyingType(type) != null)
        {
            if (_random.NextDouble() < 0.2)
                return null;
            type = Nullable.GetUnderlyingType(type)!;
        }

        if (type == typeof(string))
        {
            var guidStr = Guid.NewGuid().ToString("N");
            var length = _random.Next(4, Math.Min(12, guidStr.Length + 1));
            return guidStr[..length];
        }

        if (type == typeof(bool))
            return _random.Next(2) == 0;

        if (type == typeof(byte))
            return (byte)_random.Next(byte.MinValue, byte.MaxValue + 1);

        if (type == typeof(short))
            return (short)_random.Next(short.MinValue, short.MaxValue + 1);
        
        if (type == typeof(int))
            return _random.Next(int.MinValue, int.MaxValue);

        if (type == typeof(long))
            return _random.NextInt64(long.MinValue, long.MaxValue);

        if (type == typeof(float))
            return (float)_random.NextDouble();

        if (type == typeof(double))
            return _random.NextDouble();
        
        if (type == typeof(decimal))
            return new decimal(_random.Next(), _random.Next(), _random.Next(), _random.Next(2) == 1, (byte)_random.Next(0, 29));

        if (type == typeof(DateTime))
            return new DateTime(_random.NextInt64(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks));
        
        if (type == typeof(DateTimeOffset))
            return new DateTimeOffset(_random.NextInt64(DateTimeOffset.MinValue.Ticks, DateTimeOffset.MaxValue.Ticks), TimeSpan.Zero);
        
        if (type == typeof(TimeSpan))
            return new TimeSpan(_random.NextInt64(TimeSpan.MinValue.Ticks, TimeSpan.MaxValue.Ticks));
        
        if (type.IsEnum)
        {
            Array values = Enum.GetValues(type);
            return values.GetValue(_random.Next(values.Length));
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var arrayLength = _random.Next(1, 5);
            var array = Array.CreateInstance(elementType, arrayLength);

            visited.Add(type);
            for (int i = 0; i < arrayLength; i++)
                array.SetValue(GenerateRandom(elementType, visited), i);
            visited.Remove(type);

            return array;
        }

        if (IsGenericList(type))
        {
            var elementType = type.GetGenericArguments()[0];
            var listLength = _random.Next(1, 5);
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList?)Activator.CreateInstance(listType);
            visited.Add(type);
            for (int i = 0; i < listLength; i++)
                list?.Add(GenerateRandom(elementType, visited));
            
            visited.Remove(type);
            return list;
        }

        if (type.IsInterface || type.IsAbstract)
            return null;

        object? instance;
        try
        {
            instance = Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
        visited.Add(type);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.GetIndexParameters().Length == 0))
        {
            if (prop.PropertyType == type)
                continue;

            try
            {
                var value = GenerateRandom(prop.PropertyType, visited);
                prop.SetValue(instance, value);
            }
            catch
            {
                
            }
        }
        visited.Remove(type);
        return instance;
    }

    /// <summary>
    /// Checks if a type is a generic list or one of the common collection interfaces.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a generic list/collection, false otherwise.</returns>
    private static bool IsGenericList(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericDef = type.GetGenericTypeDefinition();
        return genericDef == typeof(List<>)
            || genericDef == typeof(IList<>)
            || genericDef == typeof(IEnumerable<>)
            || genericDef == typeof(ICollection<>);
    }
}
