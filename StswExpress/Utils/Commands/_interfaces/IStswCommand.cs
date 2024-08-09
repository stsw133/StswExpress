using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Defines a command interface that extends <see cref="ICommand"/> and includes 
/// a property to indicate whether the command is currently executing.
/// </summary>
public interface IStswCommand : ICommand
{
    /// <summary>
    /// Gets a value indicating whether the command is currently executing.
    /// </summary>
    public bool IsBusy { get; set; }
}
