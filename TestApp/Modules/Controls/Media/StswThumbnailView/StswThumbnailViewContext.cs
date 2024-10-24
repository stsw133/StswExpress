using System.Collections.Generic;
using System.IO;

namespace TestApp;

public class StswThumbnailViewContext : ControlsContext
{
    /// DirectoryPath
    public string? DirectoryPath
    {
        get => _directoryPath;
        set
        {
            SetProperty(ref _directoryPath, value);
            ItemsSource = value != null ? ([.. Directory.GetFiles(value)]) : ([]);
        }
    }
    private string? _directoryPath;

    /// ItemsSource
    public List<string> ItemsSource
    {
        get => _itemsSource;
        set => SetProperty(ref _itemsSource, value);
    }
    private List<string> _itemsSource = [];
}
