using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StswExpress;

/// <summary>
/// Helper class for resolving dependencies via Dependency Injection (DI).
/// </summary>
internal static class StswDependencyInjectionHelper
{
    private static readonly object _lock = new();
    private static readonly Dictionary<string, Type?> _typeCache = new(StringComparer.Ordinal);

    /// <summary>
    /// Attempts to resolve an instance of the specified <paramref name="type"/> from the application's <see cref="IServiceProvider"/>.
    /// Falls back to <see cref="Activator.CreateInstance(Type)"/> when DI is not configured or the service is not registered.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <returns>The resolved instance or <see langword="null"/> when instantiation is not possible.</returns>
    internal static object? Resolve(Type type)
    {
        if (type is null)
            return null;

        var instance = StswApp.ServiceProvider?.GetService(type);
        if (instance is not null)
            return instance;

        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to resolve an instance based on a fully qualified <paramref name="typeName"/>.
    /// </summary>
    /// <param name="typeName">The fully qualified name of the type to resolve.</param>
    /// <returns>The resolved instance or <see langword="null"/> when the type cannot be resolved.</returns>
    internal static object? Resolve(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        Type? type;
        lock (_lock)
            if (_typeCache.TryGetValue(typeName, out type))
                return type is not null ? Resolve(type) : null;

        type = Type.GetType(typeName, throwOnError: false)
            ?? Assembly.GetEntryAssembly()?.GetType(typeName, throwOnError: false)
            ?? AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(typeName, throwOnError: false))
                .FirstOrDefault(t => t is not null);

        lock (_lock)
            _typeCache[typeName] = type;

        return type is not null ? Resolve(type) : null;
    }
}
