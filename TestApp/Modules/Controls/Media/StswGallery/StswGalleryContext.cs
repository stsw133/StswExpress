﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswGalleryContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

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

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;
}
