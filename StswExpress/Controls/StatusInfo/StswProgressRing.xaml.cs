using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a control being a progress bar with additional features such as displaying progress as text and different states.
/// </summary>
public class StswProgressRing : ProgressBar
{
    static StswProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswProgressRing), new FrameworkPropertyMetadata(typeof(StswProgressRing)));
    }
}
