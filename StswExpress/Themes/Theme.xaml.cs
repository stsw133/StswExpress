using System;
using System.Windows;

namespace StswExpress;

/// Theme
partial class Theme
{
    public Theme()
    {
        InitializeComponent();
    }

    private ThemeColor color = ThemeColor.Default;
    public ThemeColor Color
    {
        get => color;
        set
        {
            if (color == value)
                return;
            color = value;
            SetColor(color);
        }
    }

    private void SetColor(ThemeColor color)
    {
        MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"/StswExpress;component/Themes/Brushes/{color}.xaml", UriKind.Relative) };
    }
}

/// ThemeColor
public enum ThemeColor
{
    Default = -1,
    Light,
    Dark
}
