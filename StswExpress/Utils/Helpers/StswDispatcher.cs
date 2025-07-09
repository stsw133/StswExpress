using System;
using System.Threading.Tasks;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Dispatcher helper for WPF applications.
/// </summary>
[Stsw("0.17.0")]
public static class StswDispatcher
{
    /// <summary>
    /// Run an action on the UI thread.
    /// </summary>
    /// <param name="action"></param>
    public static void Run(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null || dispatcher.CheckAccess())
            action();
        else
            dispatcher.Invoke(action);
    }

    /// <summary>
    /// Run an action on the UI thread asynchronously.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task RunAsync(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null || dispatcher.CheckAccess())
            action();
        else
            await dispatcher.InvokeAsync(action);
    }
}
