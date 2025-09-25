using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StswExpress;

/// <summary>
/// Provides methods to run actions on the UI thread.
/// </summary>
[StswInfo("0.17.0", "0.21.0")]
public static class StswDispatcher
{
    /// <summary>
    /// Run an action on the UI thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    [StswInfo("0.17.0")]
    public static void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null || dispatcher.CheckAccess())
            action();
        else
            dispatcher.Invoke(action);
    }

    /// <summary>
    /// Run an action on the UI thread asynchronously.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [StswInfo("0.17.0")]
    public static async Task RunAsync(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null || dispatcher.CheckAccess())
            action();
        else
            await dispatcher.InvokeAsync(action);
    }

    /// <summary>
    /// Run an action when the UI is idle (after Loaded event).
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [StswInfo("0.21.0")]
    public static Task RunWhenUiIsReadyAsync(Action action, CancellationToken ct = default)
        => RunWhenUiIsReadyAsync(() => { action(); return Task.CompletedTask; }, ct);

    /// <summary>
    /// Run an async action when the UI is idle (after Loaded event).
    /// </summary>
    /// <param name="actionAsync">The async action to run.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [StswInfo("0.21.0")]
    public static async Task RunWhenUiIsReadyAsync(Func<Task> actionAsync, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(actionAsync);
        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        await dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded, ct);
        await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle, ct);

        if (ct.IsCancellationRequested) return;
        await dispatcher.InvokeAsync(async () => await actionAsync(), DispatcherPriority.Normal, ct);
    }
}
