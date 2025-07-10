using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Defines a command interface that extends <see cref="ICommand"/> and includes 
/// a property to indicate whether the command is currently executing.
/// </summary>
[StswInfo("0.9.0")]
public interface IStswAsyncCommand : ICommand
{
    /// <summary>
    /// Gets or sets a value indicating whether the command is currently executing.
    /// </summary>
    public bool IsBusy { get; set; }

    /// <summary>
    /// Gets or sets the minimum possible value of the command execution.
    /// When both <see cref="Minimum"/> and <see cref="Maximum"/> properties are set to <c>0</c>,
    /// then progress ring in <see cref="StswProgressLabel"/> will become indeterminate.
    /// </summary>
    public double Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum possible value of the command execution.
    /// When both <see cref="Minimum"/> and <see cref="Maximum"/> properties are set to <c>0</c>,
    /// then progress ring in <see cref="StswProgressLabel"/> will become indeterminate.
    /// </summary>
    public double Maximum { get; set; }

    /// <summary>
    /// Gets or sets the current state of the command.
    /// </summary>
    public StswProgressState State { get; set; }

    /// <summary>
    /// Gets or sets the current magnitude of the command execution.
    /// </summary>
    public double Value { get; set; }
}
