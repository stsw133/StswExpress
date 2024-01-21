using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestApp;

public class StswMediaPlayerContext : ControlsContext
{
    /// ItemsSource
    private List<string> itemsSource = new();
    public List<string> ItemsSource
    {
        get => itemsSource;
        set => SetProperty(ref itemsSource, value);
    }

    /// SelectedPath
    private string? selectedPath;
    public string? SelectedPath
    {
        get => selectedPath;
        set
        {
            SetProperty(ref selectedPath, value);
            if (value != null)
            {
                ItemsSource = Directory.GetFiles(Directory.GetParent(value)!.FullName).ToList();
                Source = new Uri(value);
            }
        }
    }

    /// Source
    private Uri? source;
    public Uri? Source
    {
        get => source;
        set => SetProperty(ref source, value);
    }
}
