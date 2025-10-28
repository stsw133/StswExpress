using System;

namespace TestApp;
public partial class StswHyperlinkButtonContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        VerticalAlignment = System.Windows.VerticalAlignment.Top;
    }

    [StswObservableProperty] Uri _navigateUri = new("https://example.com");
}
