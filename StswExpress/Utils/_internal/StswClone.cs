using System.Collections.Generic;
using System.Reflection;
using System;
using System.Collections;

namespace StswExpress;
/// <summary>
/// Provides utility methods for deep cloning objects including copying properties and fields using reflection.
/// </summary>
internal static class StswClone
{
    /// <summary>
    /// Copies all writable properties from the original object to the new clone object using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the objects involved in the cloning process.</typeparam>
    /// <param name="original">The original object from which to copy properties.</param>
    /// <param name="cloneObject">The new clone object to which properties will be copied.</param>
    /// <param name="typeToReflect">The type of the objects being cloned. This is used to retrieve property information.</param>
    /// <remarks>
    /// Only properties that are writable and do not have index parameters are copied. This method is recursively called to ensure deep cloning of all properties.
    /// </remarks>
    internal static void CopyProperties<T>(T original, T cloneObject, Type typeToReflect)
    {
        foreach (var prop in typeToReflect.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (prop.CanWrite && prop.GetIndexParameters().Length == 0)
            {
                var originalValue = prop.GetValue(original);
                var clonedValue = CloneValue(originalValue!);
                prop.SetValue(cloneObject, clonedValue);
            }
        }
    }

    /// <summary>
    /// Copies all fields from the original object to the new clone object using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the objects involved in the cloning process.</typeparam>
    /// <param name="original">The original object from which to copy fields.</param>
    /// <param name="cloneObject">The new clone object to which fields will be copied.</param>
    /// <param name="typeToReflect">The type of the objects being cloned. This is used to retrieve field information.</param>
    /// <remarks>
    /// All fields, including private fields, are copied to ensure a complete deep clone. This method uses recursion to clone any complex types encountered.
    /// </remarks>
    internal static void CopyFields<T>(T original, T cloneObject, Type typeToReflect)
    {
        foreach (var field in typeToReflect.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var originalValue = field.GetValue(original);
            var clonedValue = CloneValue(originalValue!);
            field.SetValue(cloneObject, clonedValue);
        }
    }

    /// <summary>
    /// Clones a value, determining how to clone based on whether the value is a primitive type, a clonable object, or a collection.
    /// </summary>
    /// <param name="originalValue">The value to clone.</param>
    /// <returns>A cloned copy of the original value. If the original value is a collection, each item in the collection is also cloned.</returns>
    /// <remarks>
    /// This method decides how to clone the value based on its type. Primitive types are returned directly, clonable objects are cloned using their
    /// Clone method, and collections are cloned by creating a new collection and cloning each item.
    /// </remarks>
    internal static object? CloneValue(object originalValue)
    {
        if (originalValue == null)
            return null;

        var typeToReflect = originalValue.GetType();
        if (IsPrimitive(typeToReflect))
            return originalValue;

        if (typeToReflect.IsArray)
        {
            var elementType = typeToReflect.GetElementType();
            var originalArray = (Array)originalValue;
            var clonedArray = Array.CreateInstance(elementType!, originalArray.Length);
            for (int i = 0; i < originalArray.Length; i++)
                clonedArray.SetValue(CloneValue(originalArray.GetValue(i)!), i);
            return clonedArray;
        }

        if (originalValue is IList && typeToReflect.IsGenericType)
        {
            var elementType = typeToReflect.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var newList = (IList?)Activator.CreateInstance(listType);
            foreach (var item in (IEnumerable)originalValue)
                newList?.Add(item != null ? CloneValue(item) : null);
            return newList;
        }

        var cloneObject = Activator.CreateInstance(typeToReflect);
        CopyProperties(originalValue, cloneObject, typeToReflect);
        CopyFields(originalValue, cloneObject, typeToReflect);
        return cloneObject;
    }

    /// <summary>
    /// Determines whether a given type is a primitive, a value type, or a simple immutable type like string, DateTime, or decimal.
    /// </summary>
    /// <param name="type">The type to evaluate.</param>
    /// <returns>True if the type is considered primitive for the purposes of cloning; otherwise, false.</returns>
    /// <remarks>
    /// This method identifies types that do not require deep cloning because they either do not contain references to other objects or are immutable.
    /// </remarks>
    internal static bool IsPrimitive(Type type)
    {
        if (type == typeof(string) || type.IsPrimitive || type.IsValueType)
            return true;

        return type == typeof(DateTime) || type == typeof(decimal);
    }
}
