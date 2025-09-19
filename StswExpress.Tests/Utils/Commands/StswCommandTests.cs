using System;

namespace StswExpress.Tests.Utils.Commands;
public class StswCommandTests
{
    [Fact]
    public void Constructor_NullExecute_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StswCommand<string>(null!));
    }

    [Fact]
    public void CanExecute_ReturnsTrue_WhenCanExecuteIsNull()
    {
        var command = new StswCommand<string>(_ => { });
        Assert.True(command.CanExecute());
    }

    [Fact]
    public void CanExecute_ReturnsFalse_WhenCanExecuteIsFalse()
    {
        var command = new StswCommand<string>(_ => { }, () => false);
        Assert.False(command.CanExecute());
    }

    [Fact]
    public void Execute_InvokesDelegate_WithParameter()
    {
        string? received = null;
        var command = new StswCommand<string>(s => received = s);

        command.Execute("test");

        Assert.Equal("test", received);
    }

    [Fact]
    public void Execute_InvokesDelegate_WithDefaultParameter()
    {
        string? received = "notdefault";
        var command = new StswCommand<string>(s => received = s);

        command.Execute(null);

        Assert.Null(received);
    }

    [Fact]
    public void Execute_DoesNotInvokeDelegate_WhenCanExecuteIsFalse()
    {
        bool executed = false;
        var command = new StswCommand<string>(_ => executed = true, () => false);

        command.Execute("test");

        Assert.False(executed);
    }

    [Fact]
    public void StswCommand_WithoutParameter_ExecutesDelegate()
    {
        bool executed = false;
        var command = new StswCommand(() => executed = true);

        command.Execute();

        Assert.True(executed);
    }
    /*
    [Fact]
    public void CanExecuteChanged_Event_IsHookedToCommandManager()
    {
        var command = new StswCommand<string>(_ => { });
        bool eventAdded = false;
        EventHandler handler = (_, _) => eventAdded = true;

        command.CanExecuteChanged += handler;
        CommandManager.InvalidateRequerySuggested();
        // No direct way to assert event hookup, but this ensures no exception and event can be added/removed.
        command.CanExecuteChanged -= handler;
    }
    */
}
