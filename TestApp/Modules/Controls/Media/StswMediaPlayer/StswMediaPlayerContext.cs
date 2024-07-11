using System;
using System.Collections.Generic;
using System.IO;

namespace TestApp;

public class StswMediaPlayerContext : ControlsContext
{
    /// ItemsSource
    public List<string> ItemsSource
    {
        get => _itemsSource;
        set => SetProperty(ref _itemsSource, value);
    }
    private List<string> _itemsSource = [];

    /// SelectedPath
    public string? SelectedPath
    {
        get => _selectedPath;
        set
        {
            SetProperty(ref _selectedPath, value);
            if (value != null)
            {
                ItemsSource = [.. Directory.GetFiles(Directory.GetParent(value)!.FullName)];
                Source = new Uri(value);
            }
        }
    }
    private string? _selectedPath;

    /// Source
    public Uri? Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }
    private Uri? _source;
}
