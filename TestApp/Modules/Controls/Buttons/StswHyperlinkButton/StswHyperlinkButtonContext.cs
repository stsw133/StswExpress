using System;

namespace TestApp;
public partial class StswHyperlinkButtonContext : ControlsContext
{
    [StswObservableProperty] Uri _navigateUri = new("https://example.com");
}
