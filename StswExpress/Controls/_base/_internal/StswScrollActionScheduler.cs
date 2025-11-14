using System;
using System.Windows.Threading;

namespace StswExpress;

/// <summary>
/// Schedules actions to be executed on the dispatcher after a specified delay.
/// </summary>
internal sealed class StswScrollActionScheduler
{
    private readonly Dispatcher _dispatcher;
    private readonly TimeSpan _delay;
    private readonly DispatcherTimer _timer;
    private Action? _pendingAction;
    private DispatcherPriority _pendingPriority;

    public StswScrollActionScheduler(Dispatcher dispatcher, TimeSpan? delay = null)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _delay = delay ?? TimeSpan.FromMilliseconds(100);
        _timer = new DispatcherTimer(DispatcherPriority.Background, _dispatcher)
        {
            Interval = _delay
        };
        _timer.Tick += OnTimerTick;
    }

    public StswScrollActionScheduler(DispatcherObject dispatcherObject, TimeSpan? delay = null)
        : this(dispatcherObject?.Dispatcher ?? throw new ArgumentNullException(nameof(dispatcherObject)), delay)
    {
    }

    /// <summary>
    /// Schedules an action to be executed after the defined delay.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which to execute the action.</param>
    public void Schedule(Action action, DispatcherPriority priority)
    {
        ArgumentNullException.ThrowIfNull(action);

        _pendingAction = action;
        _pendingPriority = priority;
        _timer.Stop();
        _timer.Interval = _delay;
        _timer.Start();
    }

    /// <summary>
    /// Cancels any scheduled action.
    /// </summary>
    public void Cancel()
    {
        _timer.Stop();
        _pendingAction = null;
    }

    /// <summary>
    /// Handles the timer tick event to execute the scheduled action.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timer.Stop();
        var action = _pendingAction;
        _pendingAction = null;

        if (action != null)
            _dispatcher.InvokeAsync(action, _pendingPriority);
    }
}
