using System.Reflection;

namespace StswExpress.Commons;

/// <summary>
/// Provides functionality to perform deep cloning of objects.
/// </summary>
[StswInfo("0.12.0")]
public static class StswClone
{
    private static readonly MethodInfo? CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>
    /// Creates a deep copy of the given object. 
    /// Handles complex objects, arrays, and circular references.
    /// </summary>
    /// <param name="original">The object to be cloned.</param>
    /// <returns>A deep clone of the original object, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    public static object DeepCopy(this object original)
    {
        ArgumentNullException.ThrowIfNull(original);
        var visited = new Dictionary<object, object?>(new ReferenceEqualityComparer());
        return InternalCopy(original, visited)!;
    }

    /// <summary>
    /// Creates a deep copy of the given generic object. 
    /// Calls the non-generic Copy method internally.
    /// </summary>
    /// <typeparam name="T">The type of the object to clone.</typeparam>
    /// <param name="original">The object to be cloned.</param>
    /// <returns>A deep clone of the original object, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    public static T DeepCopy<T>(this T original)
    {
        ArgumentNullException.ThrowIfNull(original);
        var visited = new Dictionary<object, object?>(new ReferenceEqualityComparer());
        return (T)InternalCopy(original, visited)!;
    }

    /// <summary>
    /// Checks if a given type is primitive (including strings).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is primitive or a string, <see langword="false"/> otherwise.</returns>
    private static bool IsPrimitive(this Type type) => type == typeof(string) || (type.IsValueType && type.IsPrimitive);

    /// <summary>
    /// Recursively copies an object and handles circular references by tracking visited objects.
    /// </summary>
    /// <param name="originalObject">The object to be cloned.</param>
    /// <param name="visited">Dictionary to track already cloned objects.</param>
    /// <returns>A deep clone of the original object, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    private static object? InternalCopy(object? originalObject, IDictionary<object, object?> visited)
    {
        if (originalObject == null)
            return null;

        if (visited.ContainsKey(originalObject))
            return visited[originalObject];

        var typeToReflect = originalObject.GetType();

        if (typeToReflect.IsPrimitive())
            return originalObject;

        if (typeToReflect.IsArray)
        {
            var arrayType = typeToReflect.GetElementType();
            var originalArray = (Array)originalObject;
            var cloneArray = (Array?)CloneMethod?.Invoke(originalObject, null);
            visited.Add(originalObject, cloneArray);

            if (arrayType != null && !arrayType.IsPrimitive())
                cloneArray?.ForEach((array, indices) => array.SetValue(InternalCopy(originalArray.GetValue(indices), visited), indices));

            return cloneArray;
        }

        if (typeof(Delegate).IsAssignableFrom(typeToReflect))
            return null;

        var cloneObject = CloneMethod?.Invoke(originalObject, null) ?? throw new InvalidOperationException("CloneMethod returned null for a non-null object.");
        visited.Add(originalObject, cloneObject);

        CopyFields(originalObject, visited, cloneObject, typeToReflect);
        RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);

        return cloneObject;
    }

    /// <summary>
    /// Recursively copies private fields from base types of the object being cloned.
    /// </summary>
    /// <param name="originalObject">The original object to clone from.</param>
    /// <param name="visited">Dictionary to track already cloned objects.</param>
    /// <param name="cloneObject">The cloned object being populated with copied fields.</param>
    /// <param name="typeToReflect">The current type to reflect upon.</param>
    private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object?> visited, object? cloneObject, Type typeToReflect)
    {
        if (typeToReflect.BaseType != null)
        {
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
            CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        }
    }

    /// <summary>
    /// Copies fields of the given object to the clone, using reflection to access both public and non-public fields.
    /// </summary>
    /// <param name="originalObject">The original object to clone from.</param>
    /// <param name="visited">Dictionary to track already cloned objects.</param>
    /// <param name="cloneObject">The cloned object being populated with copied fields.</param>
    /// <param name="typeToReflect">The current type to reflect upon.</param>
    /// <param name="bindingFlags">The binding flags to control what fields are copied.</param>
    /// <param name="filter">An optional filter function to include/exclude specific fields.</param>
    private static void CopyFields(object originalObject, IDictionary<object, object?> visited, object? cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool>? filter = null)
    {
        foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
        {
            if (filter != null && !filter(fieldInfo))
                continue;

            if (fieldInfo.FieldType.IsPrimitive())
                continue;

            var originalFieldValue = fieldInfo.GetValue(originalObject);
            var clonedFieldValue = InternalCopy(originalFieldValue, visited);
            fieldInfo.SetValue(cloneObject, clonedFieldValue);
        }
    }

    /// <summary>
    /// Iterates over a multidimensional array, applying a specified action to each element.
    /// </summary>
    /// <param name="array">The array to iterate over.</param>
    /// <param name="action">The action to perform on each element.</param>
    private static void ForEach(this Array array, Action<Array, int[]> action)
    {
        if (array.LongLength == 0)
            return;

        var walker = new ArrayTraverse(array);
        do action(array, walker.Position);
        while (walker.Step());
    }

    /// <summary>
    /// Equality comparer that compares objects by reference, not by value.
    /// </summary>
    private class ReferenceEqualityComparer : EqualityComparer<object>
    {
        public override bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public override int GetHashCode(object obj) => obj == null ? 0 : obj.GetHashCode();
    }

    /// <summary>
    /// Helper class for traversing multidimensional arrays.
    /// </summary>
    private class ArrayTraverse
    {
        public int[] Position;
        private int[] _maxLengths;

        public ArrayTraverse(Array array)
        {
            _maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
                _maxLengths[i] = array.GetLength(i) - 1;
            Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (int i = 0; i < Position.Length; ++i)
            {
                if (Position[i] < _maxLengths[i])
                {
                    Position[i]++;
                    for (int j = 0; j < i; j++)
                        Position[j] = 0;

                    return true;
                }
            }
            return false;
        }
    }
}
