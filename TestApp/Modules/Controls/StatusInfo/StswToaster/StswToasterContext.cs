using System;
using System.Linq;

namespace TestApp;
public partial class StswToasterContext : ControlsContext
{
    public StswCommand ShowToastCommand => new(ShowToast);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsClosable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsClosable)))?.Value ?? default;
        IsCopyable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsCopyable)))?.Value ?? default;
        IsExpandable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsExpandable)))?.Value ?? default;
    }

    #region Events & methods
    /// Command: show toast
    private void ShowToast()
    {
        var type = StswDialogImage.None.GetNextValue(new Random().Next(Enum.GetValues(typeof(StswInfoType)).Length));
        StswToaster.Show(type, "Toast notification at " + DateTime.Now.ToString(), async () => await StswMessageDialog.Show(DateTime.Now.ToString(), "Toast", image: type));
    }
    #endregion

    [StswObservableProperty] bool _isClosable;
    [StswObservableProperty] bool _isCopyable;
    [StswObservableProperty] bool _isExpandable;
}
