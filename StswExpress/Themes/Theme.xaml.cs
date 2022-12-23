using System;
using System.Windows;

namespace StswExpress;

partial class Theme
{
    public Theme()
    {
        InitializeComponent();
    }

    ThemeColor color;
    public ThemeColor Color
    {
        get => color;
        set
        {
            if (color == value) return;
            color = value;
            SetColor(color);
        }
    }

    void SetColor(ThemeColor color)
    {
        MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"/StswExpress;component/Themes/Brushes/{color}.xaml", UriKind.Relative) };
    }
}
