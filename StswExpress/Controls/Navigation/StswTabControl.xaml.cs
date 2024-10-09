using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// Represents a tab control with extended functionality, including support for creating new tab items.
/// </summary>
public class StswTabControl : TabControl
{
    static StswTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabControl), new FrameworkPropertyMetadata(typeof(StswTabControl)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        NewItemCommand = new StswCommand(CreateItem);
    }

    /// <summary>
    /// Creates or identifies the container used for a tab item.
    /// </summary>
    /// <returns>A new instance of <see cref="StswTabItem"/>.</returns>
    protected override DependencyObject GetContainerForItemOverride() => new StswTabItem();

    /// <summary>
    /// Determines whether the specified item is its own container or if a new container is needed.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns><see langword="true"/> if the item is a <see cref="StswTabItem"/>; otherwise, <see langword="false"/>.</returns>
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTabItem;

    /// <summary>
    /// Creates a new tab item and adds it to the tab control.
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
    /// Gets or sets a value indicating whether the tabs are visible in the tab control.
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
    /// Gets or sets the newly added item.
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
    /// Gets or sets the command that creates a new item in the tab control.
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
    /// Gets or sets the visibility of the new item button in the tab control.
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
