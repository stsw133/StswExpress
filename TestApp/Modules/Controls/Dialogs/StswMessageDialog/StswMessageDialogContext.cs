using System.Windows.Input;

namespace TestApp;

public class StswMessageDialogContext : ControlsContext
{
    public ICommand OpenMessageDialogCommand { get; set; }

    public StswMessageDialogContext()
    {
        OpenMessageDialogCommand = new StswCommand(OpenMessageDialog);
    }

    #region Events and methods
    /// Command: open content dialog
    private void OpenMessageDialog()
    {
        MessageDialogBinding = new()
        {
            Title = "Lorem ipsum dolor sit amet...",
            Content = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc, ",
            Buttons = MessageDialogButtons,
            Image = MessageDialogImage,
            OnYesCommand = new StswCommand(DialogYes),
            OnNoCommand = new StswCommand(DialogNo),
            OnCancelCommand = new StswCommand(DialogCancel),
            //IsOpen = true
        };
        IsOpen = true;
    }

    /// Command: dialog yes
    private void DialogYes()
    {
        MessageDialogResult = "yes";
        IsOpen = false;
    }
    /// Command: dialog no
    private void DialogNo()
    {
        MessageDialogResult = "no";
        IsOpen = false;
    }
    /// Command: dialog cancel
    private void DialogCancel()
    {
        MessageDialogResult = "cancel";
        IsOpen = false;
    }
    #endregion

    #region Properties
    /// MessageDialogButtons
    private StswDialogButtons messageDialogButtons = StswDialogButtons.OK;
    public StswDialogButtons MessageDialogButtons
    {
        get => messageDialogButtons;
        set => SetProperty(ref messageDialogButtons, value);
    }

    /// MessageDialogImage
    private StswDialogImage messageDialogImage = StswDialogImage.None;
    public StswDialogImage MessageDialogImage
    {
        get => messageDialogImage;
        set => SetProperty(ref messageDialogImage, value);
    }

    /// MessageDialogModel
    private StswMessageDialogModel messageDialogBinding = new();
    public StswMessageDialogModel MessageDialogBinding
    {
        get => messageDialogBinding;
        set => SetProperty(ref messageDialogBinding, value);
    }

    /// MessageDialogResult
    private string? messageDialogResult;
    public string? MessageDialogResult
    {
        get => messageDialogResult;
        set => SetProperty(ref messageDialogResult, value);
    }

    /// IsOpen
    private bool isOpen;
    public bool IsOpen
    {
        get => isOpen;
        set => SetProperty(ref isOpen, value);
    }
    #endregion
}
