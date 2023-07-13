using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public class RgsScrollViewer : ScrollViewer
{
    #region Constructor
    static RgsScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RgsScrollViewer), new FrameworkPropertyMetadata(typeof(RgsScrollViewer)));
    }
    #endregion


}
