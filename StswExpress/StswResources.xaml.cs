﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace StswExpress;

/// <summary>
/// A class for merging all StswExpress's resource dictionaries.
/// </summary>
public partial class StswResources
{
    public StswResources()
    {
        InitializeComponent();
    }
    public StswResources(StswTheme theme)
    {
        InitializeComponent();
        Theme = theme;
    }

    /// <summary>
    /// 
    /// </summary>
    public static StswResources? GetInstance()
    {
        if (Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources) is StswResources dict)
            return dict;
        return null;
    }

    /// <summary>
    /// List of themes available in <see cref="StswConfig"/> control.
    /// </summary>
    public static ObservableCollection<StswTheme> AvailableThemes = Enum.GetValues(typeof(StswTheme)).Cast<StswTheme>().ToObservableCollection();

    /// <summary>
    /// Selected theme for application.
    /// </summary>
    public StswTheme Theme
    {
        get => theme;
        set
        {
            var newTheme = value < 0 ? StswFn.GetWindowsTheme() : value;

            if (theme == newTheme)
                return;

            theme = newTheme;
            SetTheme(newTheme);
        }
    }
    private StswTheme theme = StswTheme.Auto;

    /// <summary>
    /// Method for selecting Theme.
    /// </summary>
    private void SetTheme(StswTheme theme)
    {
        MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"/StswExpress;component/Themes/Brushes/{theme}.xaml", UriKind.Relative) };
        (Application.Current as StswApp)?.OnThemeChanged(this, theme);
    }
}
