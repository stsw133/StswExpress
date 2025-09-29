using System.Linq;
using System.Threading.Tasks;

namespace TestApp;
public partial class StswMessageDialogContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        CloseOnBackdropClick = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(CloseOnBackdropClick)))?.Value ?? default;
        MessageDialogButtons = (StswDialogButtons?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(MessageDialogButtons)))?.Value ?? default;
        MessageDialogImage = (StswDialogImage?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(MessageDialogImage)))?.Value ?? default;
    }

    [StswCommand] async Task OpenMessageDialog()
    {
        var result = await StswMessageDialog.Show(
            "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi...",
            "Lorem ipsum dolor sit amet...",
            ShowDetails ? "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc..." : null,
            MessageDialogButtons,
            MessageDialogImage,
            true,
            nameof(StswMessageDialogView));

        MessageDialogResult = result?.ToString();
    }

    [StswObservableProperty] bool _closeOnBackdropClick;
    [StswObservableProperty] StswDialogButtons _messageDialogButtons;
    [StswObservableProperty] StswDialogImage _messageDialogImage;
    [StswObservableProperty] string? _messageDialogResult;
    [StswObservableProperty] bool _showDetails;
}
