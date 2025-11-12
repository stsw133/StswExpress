using System;
using System.Linq;
using System.Windows;

namespace TestApp;
public partial class StswSplitButtonContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        VerticalAlignment = VerticalAlignment.Top;

        AutoClose = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSplitButton.AutoCloseProperty)?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSplitButton.IsReadOnlyProperty)?.Value ?? default;
        DropArrowVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property == StswDropArrow.VisibilityProperty)?.Value ?? default;
    }

    [StswCommand] void OnClick(string option) => ClickOption = Convert.ToInt32(option);

    [StswObservableProperty] bool _autoClose;
    [StswObservableProperty] int _clickOption;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] Visibility _dropArrowVisibility = Visibility.Visible;
}
