namespace StswExpress.Commons.Tests;
public class ExampleMessage : IStswMessage
{
    public string Content { get; set; } = string.Empty;
}

public class StswMessangerTests
{
    [Fact]
    public void Register_And_Send_InvokesCallback()
    {
        // Arrange
        string? received = null;
        void Handler(ExampleMessage msg) => received = msg.Content;
        StswMessanger.Instance.Register<ExampleMessage>(Handler);

        // Act
        var message = new ExampleMessage { Content = "Hello" };
        StswMessanger.Instance.Send(message);

        // Assert
        Assert.Equal("Hello", received);

        // Cleanup
        StswMessanger.Instance.Unregister<ExampleMessage>(Handler);
    }

    [Fact]
    public void Unregister_RemovesCallback()
    {
        // Arrange
        bool called = false;
        void Handler(ExampleMessage msg) => called = true;
        StswMessanger.Instance.Register<ExampleMessage>(Handler);
        StswMessanger.Instance.Unregister<ExampleMessage>(Handler);

        // Act
        var message = new ExampleMessage { Content = "Test" };
        StswMessanger.Instance.Send(message);

        // Assert
        Assert.False(called);
    }

    [Fact]
    public void MultipleHandlers_AllInvoked()
    {
        // Arrange
        int callCount = 0;
        void Handler1(ExampleMessage msg) => callCount++;
        void Handler2(ExampleMessage msg) => callCount++;
        StswMessanger.Instance.Register<ExampleMessage>(Handler1);
        StswMessanger.Instance.Register<ExampleMessage>(Handler2);

        // Act
        var message = new ExampleMessage { Content = "Multi" };
        StswMessanger.Instance.Send(message);

        // Assert
        Assert.Equal(2, callCount);

        // Cleanup
        StswMessanger.Instance.Unregister<ExampleMessage>(Handler1);
        StswMessanger.Instance.Unregister<ExampleMessage>(Handler2);
    }

    [Fact]
    public void Send_NoSubscribers_DoesNotThrow()
    {
        // Arrange
        var message = new ExampleMessage { Content = "NoSubs" };

        // Act & Assert
        var ex = Record.Exception(() => StswMessanger.Instance.Send(message));
        Assert.Null(ex);
    }

    [Fact]
    public void Register_SameHandlerTwice_InvokesTwice()
    {
        // Arrange
        int callCount = 0;
        void Handler(ExampleMessage msg) => callCount++;
        StswMessanger.Instance.Register<ExampleMessage>(Handler);
        StswMessanger.Instance.Register<ExampleMessage>(Handler);

        // Act
        var message = new ExampleMessage { Content = "Twice" };
        StswMessanger.Instance.Send(message);

        // Assert
        Assert.Equal(2, callCount);

        // Cleanup
        StswMessanger.Instance.Unregister<ExampleMessage>(Handler);
        StswMessanger.Instance.Unregister<ExampleMessage>(Handler);
    }
}
