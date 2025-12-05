using System;
using System.Linq;
using System.Windows.Media;

namespace TestApp.Wpf;
public partial class StswImageContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        MenuMode = (StswMenuMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(MenuMode)))?.Value ?? default;
        Stretch = (Stretch?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Stretch)))?.Value ?? default;
    }

    [StswObservableProperty] StswMenuMode _menuMode;
    [StswObservableProperty] Uri _source = new("/Resources/icon.ico", UriKind.Relative);
    [StswObservableProperty] Stretch _stretch;
}
