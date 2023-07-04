using System.Windows.Input;

namespace TestApp;

public class StswContentDialogContext : StswObservableObject
{
    public ICommand OpenContentDialogCommand { get; set; }

    public StswContentDialogContext()
    {
        OpenContentDialogCommand = new StswRelayCommand(OpenContentDialog);
    }

    /// Command: open content dialog
    private void OpenContentDialog()
    {
        ContentDialogBinding = new()
        {
            OnYesCommand = new StswRelayCommand(DialogYes),
            OnNoCommand = new StswRelayCommand(DialogNo),
            OnCancelCommand = new StswRelayCommand(DialogCancel),
            IsOpen = true
        };
    }

    /// Command: dialog yes
    private void DialogYes()
    {
        ContentDialogResult = "DialogResult: yes!";
        ContentDialogBinding = new();
    }
    /// Command: dialog no
    private void DialogNo()
    {
        ContentDialogResult = "DialogResult: no!";
        ContentDialogBinding = new();
    }
    /// Command: dialog cancel
    private void DialogCancel()
    {
        ContentDialogBinding = new();
    }

    /// ContentDialogModel
    private StswContentDialogModel contentDialogBinding = new();
    public StswContentDialogModel ContentDialogBinding
    {
        get => contentDialogBinding;
        set => SetProperty(ref contentDialogBinding, value);
    }

    /// ContentDialogResult
    private string? contentDialogResult;
    public string? ContentDialogResult
    {
        get => contentDialogResult;
        set => SetProperty(ref contentDialogResult, value);
    }
}
