using System;

namespace TestApp;

public class StswHyperlinkButtonContext : ControlsContext
{
    /// NavigateUri
    public Uri NavigateUri
    {
        get => _navigateUri;
        set => SetProperty(ref _navigateUri, value);
    }
    private Uri _navigateUri = new Uri("https://www.google.com/");
}
