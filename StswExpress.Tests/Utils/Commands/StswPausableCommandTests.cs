using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StswExpress.Tests;
public class StswPausableCommandTests
{
    [Fact]
    public void Constructor_NullExecute_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StswPausableCommand<string>(null!));
    }

    [Fact]
    public void SetItems_SetsItemsAndResetsIndex()
    {
        var command = new StswPausableCommand<string>((_, _) => Task.CompletedTask);
        var items = new[] { "A", "B", "C" };
        command.SetItems(items);

        Assert.Equal(items, command.Items);
        Assert.Equal(0, command.CurrentIndex);
    }

    [Fact]
    public void Items_Setter_ResetsIndex()
    {
        var command = new StswPausableCommand<string>((_, _) => Task.CompletedTask);
        var items = new List<string> { "X", "Y" };
        command.Items = items;

        Assert.Equal(items, command.Items);
        Assert.Equal(0, command.CurrentIndex);
    }

    [Fact]
    public async Task Execute_ProcessesAllItems_AndResetsState()
    {
        var processed = new List<string>();
        var command = new StswPausableCommand<string>(async (item, token) =>
        {
            processed.Add(item);
            await Task.Delay(10, token);
        });

        var items = new[] { "1", "2", "3" };
        command.SetItems(items);

        command.Execute();
        await Task.Delay(100);

        Assert.Equal(items, processed);
        Assert.False(command.IsBusy);
        Assert.Null(command.Items);
        Assert.Equal(0, command.CurrentIndex);
    }

    [Fact]
    public async Task Execute_PausesAndResumesProcessing()
    {
        var processed = new List<string>();
        var tcs = new TaskCompletionSource();
        var command = new StswPausableCommand<string>(async (item, token) =>
        {
            processed.Add(item);
            if (item == "A")
                await tcs.Task;
            else
                await Task.Delay(10, token);
        });

        command.SetItems(new[] { "A", "B" });

        // Start processing (should pause at "A")
        var execTask = Task.Run(() => command.Execute());
        await Task.Delay(30);
        Assert.True(command.IsBusy);
        Assert.Single(processed);

        // Pause
        command.Execute();
        tcs.SetResult();
        await Task.Delay(30);
        Assert.False(command.IsBusy);

        // Resume
        command.Execute();
        await Task.Delay(50);
        Assert.Equal(2, processed.Count);
        Assert.False(command.IsBusy);
    }

    [Fact]
    public void Execute_DoesNothing_IfNoItems()
    {
        var command = new StswPausableCommand<string>((_, _) => Task.CompletedTask);
        command.Execute();
        Assert.False(command.IsBusy);
    }

    [Fact]
    public void CurrentIndex_Setter_UpdatesIndex()
    {
        var command = new StswPausableCommand<string>((_, _) => Task.CompletedTask);
        command.CurrentIndex = 5;
        Assert.Equal(5, command.CurrentIndex);
    }

    [Fact]
    public async Task StswPausableCommand_WithoutParameter_ExecutesDelegate()
    {
        bool executed = false;
        var command = new StswPausableCommand(async token =>
        {
            executed = true;
            await Task.Delay(10, token);
        });

        command.Items = new object[] { new object() };
        command.Execute();
        await Task.Delay(30);

        Assert.True(executed);
    }
}
