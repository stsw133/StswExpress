using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public class StswExpandArrow : Control
{
    static StswExpandArrow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswExpandArrow), new FrameworkPropertyMetadata(typeof(StswExpandArrow)));
    }

    public bool IsExpanded
    {
        get { return (bool)GetValue(IsExpandedProperty); }
        set { SetValue(IsExpandedProperty, value); }
    }
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool), 
            typeof(StswExpandArrow), 
            new PropertyMetadata(false));


}
