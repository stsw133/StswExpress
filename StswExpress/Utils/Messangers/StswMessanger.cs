using System;
using System.Collections.Generic;

namespace StswExpress;

/// <summary>
/// A simple Messenger class for sending and receiving messages between view models.
/// </summary>
public class StswMessanger
{
    private readonly Dictionary<Type, List<Action<IStswMessage>>> _subscribers = [];

    /// <summary>
    /// Gets the singleton instance of the Messenger.
    /// </summary>
    public static StswMessanger Instance => _instance;
    private static readonly StswMessanger _instance = new();

    /// <summary>
    /// Registers a callback to be invoked when a message of type TMessage is sent.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="callback">The callback to invoke.</param>
    public void Register<TMessage>(Action<TMessage> callback) where TMessage : IStswMessage
    {
        var messageType = typeof(TMessage);
        if (!_subscribers.ContainsKey(messageType))
            _subscribers[messageType] = [];

        _subscribers[messageType].Add((message) => callback((TMessage)message));
    }

    /// <summary>
    /// Unregisters a previously registered callback for messages of type TMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="callback">The callback to remove.</param>
    public void Unregister<TMessage>(Action<TMessage> callback) where TMessage : IStswMessage
    {
        var messageType = typeof(TMessage);
        if (_subscribers.TryGetValue(messageType, out var value))
            value.Remove((message) => callback((TMessage)message));
    }

    /// <summary>
    /// Sends a message of type TMessage to all registered callbacks.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="message">The message to send.</param>
    public void Send<TMessage>(TMessage message) where TMessage : IStswMessage
    {
        var messageType = typeof(TMessage);
        if (_subscribers.TryGetValue(messageType, out var value))
        {
            foreach (var callback in value)
                callback(message);
        }
    }
}

/* usage:

public class ExampleMessage : IStswMessage
{
    public string Content { get; set; }
}

class Program
{
    static void Main()
    {
        StswMessanger.Instance.Register<ExampleMessage>(HandleExampleMessage);

        var message = new ExampleMessage { Content = "Hello from StswMessanger!" };
        StswMessanger.Instance.Send(message);

        StswMessanger.Instance.Unregister<ExampleMessage>(HandleExampleMessage);
    }

    static void HandleExampleMessage(ExampleMessage message)
    {
        Console.WriteLine($"Message received: {message.Content}");
    }
}

*/
