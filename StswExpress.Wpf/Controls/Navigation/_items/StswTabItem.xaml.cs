using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress.Wpf;
/// <summary>
/// A tab item with additional functionality, including support for a close button.
/// Allows users to remove tabs dynamically from the tab control.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTabItem Header="Documents" IsClosable="True"/&gt;
/// </code>
/// </example>
public class StswTabItem : TabItem
{
    static StswTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabItem), new FrameworkPropertyMetadata(typeof(StswTabItem)));
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets a value indicating whether the tab item can be closed by the user. 
    /// When set to <see langword="true"/>, a close button is displayed in the tab.
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

    #region Template
    private ButtonBase? _closeTabButton;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DetachTemplateEvents();
        _closeTabButton = GetTemplateChild("PART_CloseTabButton") as ButtonBase;
        AttachTemplateEvents();
    }

    /// <summary>
    /// Attaches event handlers to the template parts.
    /// </summary>
    private void AttachTemplateEvents()
    {
        if (_closeTabButton != null)
            _closeTabButton.Click += PART_CloseTabButton_Click;
    }

    /// <summary>
    /// Detaches event handlers from the template parts.
    /// </summary>
    private void DetachTemplateEvents()
    {
        if (_closeTabButton != null)
            _closeTabButton.Click -= PART_CloseTabButton_Click;
    }
    #endregion

    #region Overrides
    private bool _isResolvingContent;

    /// <inheritdoc/>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (_isResolvingContent || DesignerProperties.GetIsInDesignMode(this) || newContent is null)
        {
            base.OnContentChanged(oldContent, newContent);
            return;
        }

        object? resolved = null;

        switch (newContent)
        {
            case Type type:
                resolved = StswDependencyInjectionHelper.Resolve(type);
                break;

            case string typeName:
                resolved = StswDependencyInjectionHelper.Resolve(typeName);
                break;
        }

        if (resolved is null)
        {
            base.OnContentChanged(oldContent, newContent);
            return;
        }

        _isResolvingContent = true;
        try
        {
            Content = resolved;
        }
        finally
        {
            _isResolvingContent = false;
        }
    }
    #endregion

    #region Logic
    /// <summary>
    /// Handles the click event of the close tab button.
    /// Removes the current tab item from its parent <see cref="StswTabControl"/>.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    public void PART_CloseTabButton_Click(object sender, RoutedEventArgs e)
    {
        if (StswFnUI.FindVisualAncestor<StswTabControl>(this) is StswTabControl tabControl)
        {
            if (tabControl.ItemsSource is IList list)
                list.Remove(tabControl.ItemContainerGenerator.ItemFromContainer(this));
            else
                tabControl.Items?.Remove(this);
        }
    }
    #endregion
}
