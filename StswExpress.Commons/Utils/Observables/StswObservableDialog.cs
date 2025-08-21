namespace StswExpress.Commons;
/// <summary>
/// Base class for observable dialog objects that can be used in MVVM pattern.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// public class MainDialog : StswObservableDialog
/// {
///     private string _message;
///     public string Message
///     {
///         get => _message;
///         set => SetProperty(ref _message, value);
///     }
/// }
/// </code>
/// </example>
[StswInfo("0.19.0", "0.20.0")]
public abstract class StswObservableDialog : StswObservableValidator, IDisposable
{
    public string? DialogIdentifier { get; set; }

    /// <summary>
    /// Disposes resources used by the object and suppresses finalization.
    /// </summary>
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
