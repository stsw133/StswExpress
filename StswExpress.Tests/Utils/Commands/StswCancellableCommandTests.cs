using System;
using System.Threading;
using System.Threading.Tasks;

namespace StswExpress.Tests.Utils.Commands;
public class StswCancellableCommandTests
{
    [Fact]
    public void Constructor_NullExecute_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StswCancellableCommand<string>(null!));
    }
    /*
    [Fact]
    public void CanExecute_ReturnsTrue_WhenCanExecuteIsNull_AndNotBusy()
    {
        var command = new StswCancellableCommand<string>((_, _) => Task.CompletedTask);
        command.IsBusy = false;
        Assert.True(command.CanExecute());
    }

    [Fact]
    public void CanExecute_ReturnsFalse_WhenCanExecuteIsFalse()
    {
        var command = new StswCancellableCommand<string>((_, _) => Task.CompletedTask, () => false);
        command.IsBusy = false;
        Assert.False(command.CanExecute());
    }

    [Fact]
    public void CanExecute_ReturnsTrue_WhenIsBusy()
    {
        var command = new StswCancellableCommand<string>((_, _) => Task.CompletedTask, () => false);
        command.IsBusy = true;
        Assert.True(command.CanExecute());
    }
    */
    [Fact]
    public async Task ExecuteAsync_InvokesExecuteDelegate_WithParameter()
    {
        string? received = null;
        var command = new StswCancellableCommand<string>(async (s, _) =>
        {
            await Task.Delay(10);
            received = s;
        });

        command.Execute("test");
        await Task.Delay(50);

        Assert.Equal("test", received);
    }

    [Fact]
    public async Task ExecuteAsync_InvokesExecuteDelegate_WithDefaultParameter()
    {
        string? received = "notdefault";
        var command = new StswCancellableCommand<string>(async (s, _) =>
        {
            await Task.Delay(10);
            received = s;
        });

        command.Execute(null);
        await Task.Delay(50);

        Assert.Null(received);
    }

    [Fact]
    public async Task Execute_SetsIsBusyDuringExecution()
    {
        var tcs = new TaskCompletionSource();
        var command = new StswCancellableCommand<string>(async (_, _) =>
        {
            await tcs.Task;
        });

        var task = Task.Run(() => command.Execute("busytest"));
        await Task.Delay(10);

        Assert.True(command.IsBusy);

        tcs.SetResult();
        await task;
        Assert.False(command.IsBusy);
    }

    [Fact]
    public async Task Execute_CancelsOngoingOperation()
    {
        var tcs = new TaskCompletionSource();
        bool wasCancelled = false;
        var command = new StswCancellableCommand<string>(async (_, token) =>
        {
            try
            {
                await Task.Delay(5000, token);
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
            }
        });

        command.Execute("canceltest");
        await Task.Delay(50);

        command.Execute("canceltest"); // Should trigger cancellation
        await Task.Delay(50);

        Assert.True(wasCancelled);
        Assert.False(command.IsBusy);
    }

    [Fact]
    public async Task CancelAndWaitAsync_CancelsAndWaits()
    {
        var tcs = new TaskCompletionSource();
        bool wasCancelled = false;
        var command = new StswCancellableCommand<string>(async (_, token) =>
        {
            try
            {
                await Task.Delay(5000, token);
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
            }
        });

        command.Execute("cancelwait");
        await Task.Delay(50);

        await command.CancelAndWaitAsync();

        Assert.True(wasCancelled);
        Assert.False(command.IsBusy);
    }

    [Fact]
    public async Task StswCancellableCommand_WithoutParameter_ExecutesDelegate()
    {
        bool executed = false;
        var command = new StswCancellableCommand(async _ =>
        {
            await Task.Delay(10);
            executed = true;
        });

        command.Execute();
        await Task.Delay(50);

        Assert.True(executed);
    }
}
