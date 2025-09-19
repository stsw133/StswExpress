using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A navigation control that manages multiple contexts and navigation elements.
/// Supports pinned items, compact/full modes, and dynamic content switching.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswNavigationView TabStripMode="Full"&gt;
///     &lt;se:StswNavigationViewItem Header="Dashboard"/&gt;
///     &lt;se:StswNavigationViewItem Header="Settings"/&gt;
/// &lt;/se:StswNavigationView&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Items))]
[StswInfo("0.19.0", Changes = StswPlannedChanges.Finish)]
public class StswNavigationView : ContentControl, IStswCornerControl
{
    private StswNavigationTree? _mainTree, _pinnedTree;

    public StswNavigationView()
    {
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());
        SetValue(InstancesProperty, new Dictionary<StswNavigationViewItem, object>());
        SetValue(ItemsProperty, new List<object>());
        SetValue(ItemsPinnedProperty, new List<object>());
    }
    static StswNavigationView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationView), new FrameworkPropertyMetadata(typeof(StswNavigationView)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// buttons
        if (GetTemplateChild("PART_StripModeButton") is ToggleButton stripModeButton)
        {
            stripModeButton.IsChecked = TabStripMode == StswCompactibility.Full;
            stripModeButton.Checked += (_, _) => TabStripMode = StswCompactibility.Full;
            stripModeButton.Unchecked += (_, _) => TabStripMode = StswCompactibility.Compact;
        }
        /// trees
        if (GetTemplateChild("PART_MainTree") is StswNavigationTree mainTree)
        {
            _mainTree = mainTree;
            _mainTree.SelectedItemChanged += MainTree_SelectedItemChanged;
        }
        if (GetTemplateChild("PART_PinnedTree") is StswNavigationTree pinnedTree)
        {
            _pinnedTree = pinnedTree;
            _pinnedTree.SelectedItemChanged += PinnedTree_SelectedItemChanged;
        }
    }

    /// <summary>
    /// Handles the selection change in the main tree view.
    /// </summary>
    /// <param name="sender">The sender of the event, typically the main tree view.</param>
    /// <param name="e">The event arguments containing the new selected item.</param>
    private void MainTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue != null && _pinnedTree != null)
            DeselectTreeView(_pinnedTree);
    }

    /// <summary>
    /// Handles the selection change in the pinned tree view.
    /// </summary>
    /// <param name="sender">The sender of the event, typically the pinned tree view.</param>
    /// <param name="e">The event arguments containing the new selected item.</param>
    private void PinnedTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue != null && _mainTree != null)
            DeselectTreeView(_mainTree);
    }

    /// <summary>
    /// Recursively deselects the specified item and its children in the tree view.
    /// </summary>
    /// <param name="currentItem">The current tree view item to deselect.</param>
    /// <returns><see langword="true"/> if the item was deselected, <see langword="false"/> otherwise.</returns>
    private static bool DeselectItem(TreeViewItem? currentItem)
    {
        if (currentItem == null)
            return false;

        if (currentItem.IsSelected)
        {
            currentItem.IsSelected = false;
            return true;
        }

        //currentItem.IsExpanded = true;
        //currentItem.UpdateLayout();

        foreach (var child in currentItem.Items)
        {
            var childItem = currentItem.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
            if (childItem != null && DeselectItem(childItem))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Deselects all items in the specified tree view.
    /// </summary>
    /// <param name="treeView">The tree view from which to deselect items.</param>
    /// <returns><see langword="true"/> if any item was deselected, <see langword="false"/> otherwise.</returns>
    private static bool DeselectTreeView(TreeView treeView)
    {
        foreach (var item in treeView.Items)
        {
            var treeViewItem = treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            if (DeselectItem(treeViewItem))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Sets the content of the navigation view to an instance of the specified type.
    /// </summary>
    /// <param name="type">The type of the content to set.</param>
    /// <param name="isNewInstance">Determines whether to create a new instance of the type.</param>
    public void SetContent(StswNavigationViewItem item, Type? type, bool isNewInstance)
    {
        if (DesignerProperties.GetIsInDesignMode(this) || type is null)
            return;

        if (Command is not null)
        {
            Command.Execute(type);
            return;
        }

        if (isNewInstance)
        {
            Content = Activator.CreateInstance(type);
            return;
        }

        if (!Instances.TryGetValue(item, out var instance))
        {
            instance = Activator.CreateInstance(type)!;
            Instances[item] = instance;
        }
        Content = instance;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the collection of UI elements that are part of the navigation view.
    /// </summary>
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswNavigationView)
        );

    /// <summary>
    /// Gets or sets the command to execute when changing contexts.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(StswNavigationView)
        );

    /// <summary>
    /// Gets or sets a dictionary of instances for different types.
    /// </summary>
    public IDictionary<StswNavigationViewItem, object> Instances
    {
        get => (IDictionary<StswNavigationViewItem, object>)GetValue(InstancesProperty);
        set => SetValue(InstancesProperty, value);
    }
    public static readonly DependencyProperty InstancesProperty
        = DependencyProperty.Register(
            nameof(Instances),
            typeof(IDictionary<StswNavigationViewItem, object>),
            typeof(StswNavigationView)
        );

    /// <summary>
    /// Gets or sets the collection of navigation items.
    /// </summary>
    public IList Items
    {
        get => (IList)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(IList),
            typeof(StswNavigationView)
        );

    /// <summary>
    /// Gets or sets the collection of pinned navigation items.
    /// </summary>
    public IList ItemsPinned
    {
        get => (IList)GetValue(ItemsPinnedProperty);
        set => SetValue(ItemsPinnedProperty, value);
    }
    public static readonly DependencyProperty ItemsPinnedProperty
        = DependencyProperty.Register(
            nameof(ItemsPinned),
            typeof(IList),
            typeof(StswNavigationView)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the navigation shows elements and their names.
    /// Controls the navigation layout between compact and full modes.
    /// </summary>
    public StswCompactibility TabStripMode
    {
        get => (StswCompactibility)GetValue(TabStripModeProperty);
        set => SetValue(TabStripModeProperty, value);
    }
    public static readonly DependencyProperty TabStripModeProperty
        = DependencyProperty.Register(
            nameof(TabStripMode),
            typeof(StswCompactibility),
            typeof(StswNavigationView)
        );

    /// <summary>
    /// Gets or sets the alignment of navigation elements.
    /// Determines the placement of the tab strip within the control.
    /// </summary>
    public Dock TabStripPlacement
    {
        get => (Dock)GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }
    public static readonly DependencyProperty TabStripPlacementProperty
        = DependencyProperty.Register(
            nameof(TabStripPlacement),
            typeof(Dock),
            typeof(StswNavigationView)
        );
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswNavigationView),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigationView),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between items and content.
    /// Affects the spacing and visual separation in the navigation layout.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswNavigationView),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the width of the navigation items list.
    /// Adjusts the size of the tab strip for a custom layout.
    /// </summary>
    public double TabStripWidth
    {
        get => (double)GetValue(TabStripWidthProperty);
        set => SetValue(TabStripWidthProperty, value);
    }
    public static readonly DependencyProperty TabStripWidthProperty
        = DependencyProperty.Register(
            nameof(TabStripWidth),
            typeof(double),
            typeof(StswNavigationView),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );
    #endregion
}
