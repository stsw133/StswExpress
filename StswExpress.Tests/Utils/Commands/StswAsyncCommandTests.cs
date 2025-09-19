using System;
using System.Threading.Tasks;

namespace StswExpress.Tests.Utils.Commands;
public class StswAsyncCommandTests
{
    [Fact]
    public void Constructor_NullExecute_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StswAsyncCommand<string>(null!));
    }

    [Fact]
    public void CanExecute_ReturnsTrue_WhenCanExecuteIsNull_AndNotBusy()
    {
        var command = new StswAsyncCommand<string>(_ => Task.CompletedTask);
        Assert.True(command.CanExecute());
    }

    [Fact]
    public void CanExecute_ReturnsFalse_WhenCanExecuteIsFalse()
    {
        var command = new StswAsyncCommand<string>(_ => Task.CompletedTask, () => false);
        Assert.False(command.CanExecute());
    }
    /*
    [Fact]
    public void CanExecute_ReturnsFalse_WhenIsBusy_AndNotReusable()
    {
        var command = new StswAsyncCommand<string>(_ => Task.CompletedTask);
        command.IsBusy = true;
        command.IsReusable = false;
        Assert.False(command.CanExecute());
    }

    [Fact]
    public void CanExecute_ReturnsTrue_WhenIsBusy_AndReusable()
    {
        var command = new StswAsyncCommand<string>(_ => Task.CompletedTask);
        command.IsBusy = true;
        command.IsReusable = true;
        Assert.True(command.CanExecute());
    }
    */
    [Fact]
    public async Task ExecuteAsync_InvokesExecuteDelegate_WithParameter()
    {
        string? received = null;
        var command = new StswAsyncCommand<string>(async s =>
        {
            await Task.Delay(10);
            received = s;
        });

        command.Execute("test");
        await Task.Delay(50); // Allow async to complete

        Assert.Equal("test", received);
    }

    [Fact]
    public async Task ExecuteAsync_InvokesExecuteDelegate_WithDefaultParameter()
    {
        string? received = "notdefault";
        var command = new StswAsyncCommand<string>(async s =>
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
        var command = new StswAsyncCommand<string>(async _ =>
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
    public void StswAsyncCommand_WithoutParameter_ExecutesDelegate()
    {
        bool executed = false;
        var command = new StswAsyncCommand(async () =>
        {
            await Task.Delay(10);
            executed = true;
        });

        command.Execute();
        Task.Delay(50).Wait();

        Assert.True(executed);
    }
}
