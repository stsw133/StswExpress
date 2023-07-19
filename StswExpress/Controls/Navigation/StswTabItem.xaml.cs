using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that extends the <see cref="TabItem"/> class with additional functionality.
/// </summary>
public class StswTabItem : TabItem
{
    static StswTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabItem), new FrameworkPropertyMetadata(typeof(StswTabItem)));
    }

    #region Events
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        /// FunctionButton
        if (GetTemplateChild("PART_FunctionButton") is StswButton btn)
            btn.Click += PART_FunctionButton_Click;

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Handles the click event of the PART_FunctionButton.
    /// Removes the current tab item from the parent StswTabControl.
    /// </summary>
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
    /// <summary>
    /// Gets or sets a value indicating whether the tab item is closable and has a close button.
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
