namespace StswExpress.Commons;
/// <summary>
/// Base class for observable dialog objects that can be used in MVVM pattern.
/// </summary>
[Stsw("0.19.0", Changes = StswPlannedChanges.None)]
public abstract class StswObservableDialog : StswObservableObject
{
    public string? DialogIdentifier { get; set; }
}

/* usage:

public class MainDialog : StswObservableDialog
{
    private string _message;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
}

*/
