namespace StswExpress.Commons;

/// <summary>
/// A simple messenger class for sending and receiving messages within an application.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// public class ExampleMessage : IStswMessage
/// {
///     public string Content { get; set; }
/// }
/// class Program
/// {
///     static void Main()
///     {
///         StswMessanger.Instance.Register&lt;ExampleMessage&gt;(HandleExampleMessage);
/// 
///         var message = new ExampleMessage { Content = "Hello from StswMessanger!" };
///         StswMessanger.Instance.Send(message);
/// 
///         StswMessanger.Instance.Unregister&lt;ExampleMessage&gt;(HandleExampleMessage);
///     }
/// 
///     static void HandleExampleMessage(ExampleMessage message)
///     {
///         Console.WriteLine($"Message received: {message.Content}");
///     }
/// }
/// </code>
/// </example>
[StswInfo("0.9.2")]
public class StswMessanger
{
    private readonly Dictionary<Type, List<Action<IStswMessage>>> _subscribers = [];
    private readonly Dictionary<Delegate, Action<IStswMessage>> _wrappers = [];

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

        Action<IStswMessage> wrapper = (message) => callback((TMessage)message);
        _wrappers[callback] = wrapper;
        _subscribers[messageType].Add(wrapper);
    }

    /// <summary>
    /// Unregisters a previously registered callback for messages of type TMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="callback">The callback to remove.</param>
    public void Unregister<TMessage>(Action<TMessage> callback) where TMessage : IStswMessage
    {
        var messageType = typeof(TMessage);
        if (_wrappers.TryGetValue(callback, out var wrapper))
        {
            if (_subscribers.TryGetValue(messageType, out var value))
                value.Remove(wrapper);
            _wrappers.Remove(callback);
        }
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
