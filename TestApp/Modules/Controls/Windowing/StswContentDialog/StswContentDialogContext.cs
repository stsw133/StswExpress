using System;
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
            Title = StswFn.AppNameAndVersion,
            Content = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc, ",
            Button = StswFn.GetNextEnumValue(StswContentDialog.Buttons.OK, new Random().Next(0, Enum.GetValues(typeof(StswContentDialog.Buttons)).Length)),
            Image = StswFn.GetNextEnumValue(StswContentDialog.Images.None, new Random().Next(0, Enum.GetValues(typeof(StswContentDialog.Images)).Length)),
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
