using System;

namespace StswExpress;
/// <summary>
/// Provides a mechanism to defer an action until the object is disposed.
/// </summary>
/// <param name="onDispose">The action to perform when the object is disposed.</param>
public class StswRefreshBlocker(Action onDispose) : IDisposable
{
    private readonly Action _onDispose = onDispose;
    private bool _disposed;

    /// <summary>
    /// Performs the deferred action and marks the object as disposed.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _onDispose();
            _disposed = true;
        }
    }
}
