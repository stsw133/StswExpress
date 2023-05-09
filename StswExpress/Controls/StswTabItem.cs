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
        if (GetTemplateChild("PART_FunctionButton") is Button btn)
            btn.Click += PART_FunctionButton_Click;

        base.OnApplyTemplate();
    }

    /// PART_FunctionButton_Click
    public void PART_FunctionButton_Click(object sender, RoutedEventArgs e)
    {
        var tabControl = StswFn.FindVisualAncestor<StswTabControl>(this);
        if (tabControl?.ItemsSource != null)
            (tabControl?.ItemsSource as IList)?.Remove(this);
        else
            tabControl?.Items?.Remove(this);
    }
    #endregion

    #region Properties
    /// Closable
    public static readonly DependencyProperty ClosableProperty
        = DependencyProperty.Register(
            nameof(Closable),
            typeof(bool),
            typeof(StswTabItem)
        );
    public bool Closable
    {
        get => (bool)GetValue(ClosableProperty);
        set => SetValue(ClosableProperty, value);
    }
    #endregion
}
