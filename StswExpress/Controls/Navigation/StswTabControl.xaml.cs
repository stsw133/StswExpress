using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// A tab control with extended functionality, including dynamic tab creation, 
/// visibility toggling, and command-based item management.
/// </summary>
public class StswTabControl : TabControl
{
    static StswTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabControl), new FrameworkPropertyMetadata(typeof(StswTabControl)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswTabItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTabItem;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        NewItemCommand = new StswCommand(CreateItem);
    }

    /// <summary>
    /// Creates a new tab item and adds it to the tab control. 
    /// Supports both bound item sources and direct tab item collections.
    /// </summary>
    private void CreateItem()
    {
        if (ItemsSource is IList list)
        {
            var itemType = list.GetType().GenericTypeArguments.FirstOrDefault() ?? typeof(object);
            NewItem = Activator.CreateInstance(itemType);
            list.Add(NewItem);
            SelectedIndex = list.Count - 1;
        }
        else if (Items != null)
        {
            NewItem = new StswTabItem();
            Items.Add(NewItem);
            SelectedIndex = Items.Count - 1;
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the tab headers are visible in the tab control.
    /// </summary>
    public bool AreTabsVisible
    {
        get => (bool)GetValue(AreTabsVisibleProperty);
        set => SetValue(AreTabsVisibleProperty, value);
    }
    public static readonly DependencyProperty AreTabsVisibleProperty
        = DependencyProperty.Register(
            nameof(AreTabsVisible),
            typeof(bool),
            typeof(StswTabControl)
        );

    /// <summary>
    /// Gets or sets the newly created tab item when a new tab is added.
    /// </summary>
    public object? NewItem
    {
        get => (object?)GetValue(NewItemProperty);
        set => SetValue(NewItemProperty, value);
    }
    public static readonly DependencyProperty NewItemProperty
        = DependencyProperty.Register(
            nameof(NewItem),
            typeof(object),
            typeof(StswTabControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

    /// <summary>
    /// Gets or sets the command responsible for creating a new tab item in the tab control.
    /// </summary>
    public ICommand? NewItemCommand
    {
        get => (ICommand?)GetValue(NewItemCommandProperty);
        set => SetValue(NewItemCommandProperty, value);
    }
    public static readonly DependencyProperty NewItemCommandProperty
        = DependencyProperty.Register(
            nameof(NewItemCommand),
            typeof(ICommand),
            typeof(StswTabControl)
        );

    /// <summary>
    /// Gets or sets the visibility of the button used for adding new tab items.
    /// </summary>
    public Visibility NewItemButtonVisibility
    {
        get => (Visibility)GetValue(NewItemButtonVisibilityProperty);
        set => SetValue(NewItemButtonVisibilityProperty, value);
    }
    public static readonly DependencyProperty NewItemButtonVisibilityProperty
        = DependencyProperty.Register(
            nameof(NewItemButtonVisibility),
            typeof(Visibility),
            typeof(StswTabControl)
        );
    #endregion
}

/* usage:

<se:StswTabControl AreTabsVisible="True" NewItemButtonVisibility="Visible">
    <se:StswTabItem Header="Home"/>
    <se:StswTabItem Header="Settings"/>
</se:StswTabControl>

*/
