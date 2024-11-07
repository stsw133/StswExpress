using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;
/// <summary>
/// Represents a tab item with additional functionality, including support for a close button.
/// </summary>
public class StswTabItem : TabItem
{
    static StswTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabItem), new FrameworkPropertyMetadata(typeof(StswTabItem)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswTabItem), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// CloseTabButton
        if (GetTemplateChild("PART_CloseTabButton") is ButtonBase closeTabButton)
            closeTabButton.Click += PART_CloseTabButton_Click;
    }

    /// <summary>
    /// Handles the click event of the close tab button. Removes the current tab item from the parent tab control.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    public void PART_CloseTabButton_Click(object sender, RoutedEventArgs e)
    {
        if (StswFn.FindVisualAncestor<StswTabControl>(this) is StswTabControl tabControl)
        {
            if (tabControl.ItemsSource is IList list)
                list.Remove(tabControl.ItemContainerGenerator.ItemFromContainer(this));
            else
                tabControl.Items?.Remove(this);
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the tab item can be closed by the user. When true, a close button is displayed.
    /// </summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }
    public static readonly DependencyProperty IsClosableProperty
        = DependencyProperty.Register(
            nameof(IsClosable),
            typeof(bool),
            typeof(StswTabItem)
        );
    #endregion
}
