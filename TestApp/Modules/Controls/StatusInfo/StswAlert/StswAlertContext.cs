using System;
using System.Linq;

namespace TestApp;

public class StswAlertContext : ControlsContext
{
    public StswCommand ShowAlertCommand => new(ShowAlert);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsClosable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsClosable)))?.Value ?? default;
        IsCopyable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsCopyable)))?.Value ?? default;
        IsExpandable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsExpandable)))?.Value ?? default;
    }

    #region Events & methods
    /// Command: open message dialog
    private void ShowAlert()
    {
        StswAlert.Show(DateTime.Now.ToString(), async () => await StswMessageDialog.Show(DateTime.Now.ToString()));
    }
    #endregion

    /// IsClosable
    public bool IsClosable
    {
        get => _isClosable;
        set => SetProperty(ref _isClosable, value);
    }
    private bool _isClosable;
    
    /// IsCopyable
    public bool IsCopyable
    {
        get => _isCopyable;
        set => SetProperty(ref _isCopyable, value);
    }
    private bool _isCopyable;

    /// IsExpandable
    public bool IsExpandable
    {
        get => _isExpandable;
        set => SetProperty(ref _isExpandable, value);
    }
    private bool _isExpandable;
}
