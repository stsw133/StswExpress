using System;

namespace TestApp;

public class StswHyperlinkButtonContext : ControlsContext
{
    /// NavigateUri
    private Uri navigateUri = new Uri("https://www.google.com/");
    public Uri NavigateUri
    {
        get => navigateUri;
        set => SetProperty(ref navigateUri, value);
    }
}
