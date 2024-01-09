using System;

namespace TestApp;

public class StswHyperlinkButtonContext : ControlsContext
{
    /// NavigateUri
    private Uri? navigateUri;
    public Uri? NavigateUri
    {
        get => navigateUri;
        set => SetProperty(ref navigateUri, value);
    }
}
