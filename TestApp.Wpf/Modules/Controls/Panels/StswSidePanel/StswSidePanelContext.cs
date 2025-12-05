using System;

namespace TestApp.Wpf;
public partial class StswSidePanelContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));

    [StswObservableProperty] int _clickOption;
}
