using System;
using System.Linq;
using System.Windows.Media;

namespace TestApp.Wpf;
public partial class StswGifImageContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Stretch = (Stretch?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Stretch)))?.Value ?? default;
    }

    [StswObservableProperty] Uri _source = new("/Resources/icon.ico", UriKind.Relative);
    [StswObservableProperty] Stretch _stretch;
}
