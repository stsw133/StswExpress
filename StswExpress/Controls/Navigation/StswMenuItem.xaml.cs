using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswMenuItem : MenuItem
{
    static StswMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMenuItem), new FrameworkPropertyMetadata(typeof(StswMenuItem)));
    }
}
