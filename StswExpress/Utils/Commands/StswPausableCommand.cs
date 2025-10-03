using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StswExpress;

/// <summary>
/// A pausable async command that processes a collection of items iteratively with support for pause and resume.
/// The command acts as a toggle: the first call to <see cref="Execute"/> starts or resumes the operation, and the next call pauses it (cancels).
/// Inherits from <see cref="StswCancellableAsyncCommand{T}"/> and reuses its cancellation logic.
/// </summary>
/// <typeparam name="T">Type of the item to process.</typeparam>
/// <param name="executeItem">Asynchronous action to execute for each item.</param>
/// <param name="canExecute">Optional function to determine whether the command can execute. Default is <see langword="null"/>.</param>
/// <example>
/// The following example demonstrates how to use the command:
/// <code>
/// public StswPausableCommand&lt;string&gt; ProcessCommand { get; }
///
/// public MainViewModel()
/// {
///     ProcessCommand = new StswPausableCommand&lt;string&gt;(ProcessItem, () => true);
///     ProcessCommand.SetItems(new[] { "Item 1", "Item 2", "Item 3" });
/// }
///
/// private async Task ProcessItem(string item, CancellationToken token)
/// {
///     StatusMessage = $"Processing: {item}";
///     await Task.Delay(500, token);
/// }
///
/// // When ProcessCommand.Execute() is first called, it starts processing items.
/// // When called again, it pauses the operation.
/// // Calling it again resumes from where it left off.
/// </code>
/// </example>
[StswPlannedChanges(StswPlannedChanges.Remove)]
public class StswPausableCommand<T>(Func<T, CancellationToken, Task> executeItem, Func<bool>? canExecute = null) : StswCancellableCommand<T>((_, _) => Task.CompletedTask, canExecute)
{
    private readonly Func<T, CancellationToken, Task> _executeItem = executeItem ?? throw new ArgumentNullException(nameof(executeItem));
    private IReadOnlyList<T>? _items;
    private int _currentIndex = 0;

    /// <summary>
    /// Sets the items to be processed.
    /// </summary>
    public void SetItems(IEnumerable<T> items)
    {
        _items = items is IReadOnlyList<T> list ? list : [.. items];
        _currentIndex = 0;
    }

    /// <summary>
    /// Executes the command asynchronously with the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the command.</param>
    public override async void Execute(object? parameter = null)
    {
        if (IsBusy)
        {
            await CancelAndWaitAsync();
            return;
        }

        if (_items is null || _items.Count == 0 || _currentIndex >= _items.Count)
            return;

        CancellationTokenSource = new CancellationTokenSource();
        IsBusy = true;
        UpdateCanExecute();

        ExecutionTask = ProcessItemsAsync(CancellationTokenSource.Token);

        try
        {
            await ExecutionTask;
        }
        catch (OperationCanceledException)
        {
            // Paused
        }
        finally
        {
            IsBusy = false;
            UpdateCanExecute();
            CancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Internal logic to process items one by one.
    /// </summary>
    private async Task ProcessItemsAsync(CancellationToken token)
    {
        for (; _items is not null && _currentIndex < _items.Count; _currentIndex++)
        {
            token.ThrowIfCancellationRequested();
            await _executeItem(_items[_currentIndex], token);
        }

        _currentIndex = 0;
        _items = null;
    }

    /// <summary>
    /// Gets or sets the current processing index.
    /// Can be used externally to track or manipulate progress manually.
    /// </summary>
    public int CurrentIndex
    {
        get => _currentIndex;
        set => SetProperty(ref _currentIndex, value);
    }

    /// <summary>
    /// Gets or sets the currently assigned item list.
    /// </summary>
    public IReadOnlyList<T>? Items
    {
        get => _items;
        set
        {
            SetProperty(ref _items, value);
            _currentIndex = 0;
        }
    }
}

/// <summary>
/// An async command implementation (without parameter) that can be used to bind to UI controls asynchronously with <see cref="Task"/> in order to execute a given action when triggered.
/// </summary>
/// <param name="execute">The asynchronous action to execute when the command is triggered.</param>
/// <param name="canExecute">The function to determine whether the command can execute. Default is <see langword="null"/>.</param>
public class StswPausableCommand(Func<CancellationToken, Task> execute, Func<bool>? canExecute = null)
    : StswPausableCommand<object>((_, token) => execute(token), canExecute);
