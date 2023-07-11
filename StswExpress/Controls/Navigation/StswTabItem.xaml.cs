using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public class StswTabItem : TabItem
{
    static StswTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabItem), new FrameworkPropertyMetadata(typeof(StswTabItem)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// FunctionButton
        if (GetTemplateChild("PART_FunctionButton") is StswButton btn)
            btn.Click += PART_FunctionButton_Click;

        base.OnApplyTemplate();
    }

    /// PART_FunctionButton_Click
    public void PART_FunctionButton_Click(object sender, RoutedEventArgs e)
    {
        var tabControl = StswFn.FindVisualAncestor<StswTabControl>(this);
        if (tabControl != null)
        {
            if (tabControl.ItemsSource is IList list and not null)
                list.Remove(this);
            else
                tabControl.Items?.Remove(this);
        }
    }
    #endregion

    #region Main properties
    /// IsClosable
    public static readonly DependencyProperty IsClosableProperty
        = DependencyProperty.Register(
            nameof(IsClosable),
            typeof(bool),
            typeof(StswTabItem)
        );
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }
    #endregion
}
