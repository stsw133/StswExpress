using System.Windows.Input;

namespace StswExpress;
public interface IStswCommand : ICommand
{
    /// <summary>
    /// Gets a value indicating whether the command is currently executing.
    /// </summary>
    public bool IsBusy { get; set; }
}
