using System;
using System.Collections.Generic;
using System.IO;

namespace TestApp;
public partial class StswMediaPlayerContext : ControlsContext
{
    [StswObservableProperty] List<string> _itemsSource = [];
    [StswObservableProperty] Uri? _source;

    [StswObservableProperty] string? _selectedPath;
    partial void OnSelectedPathChanged(string? oldValue, string? newValue)
    {
        if (newValue != null)
        {
            ItemsSource = [.. Directory.GetFiles(Directory.GetParent(newValue)!.FullName)];
            Source = new Uri(newValue);
        }
    }
}
