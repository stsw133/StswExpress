using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A hierarchical file tree control for navigating directories and files.
/// Supports dynamic folder expansion and custom root paths.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswPathTree InitialPath="C:\" SelectedPath="{Binding SelectedFile}" ShowFiles="True"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedPath))]
[StswInfo("0.13.0", Changes = StswPlannedChanges.Fix | StswPlannedChanges.NewFeatures | StswPlannedChanges.Refactor)]
public class StswPathTree : TreeView, IStswCornerControl, IStswSelectionControl
{
    public StswPathTree()
    {
        AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnTreeViewItemExpanded));
    }
    static StswPathTree()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPathTree), new FrameworkPropertyMetadata(typeof(StswPathTree)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswTreeViewItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTreeViewItem;

    #region Events & methods
    /// <summary>
    /// Occurs when the selected path in the control changes.
    /// </summary>
    public event EventHandler? SelectedPathChanged;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        OnInitialPathChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <inheritdoc/>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <inheritdoc/>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (!IStswSelectionControl.PreviewKeyDown(this, e)) return;
        base.OnPreviewKeyDown(e);
    }

    /// <inheritdoc/>
    protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
    {
        base.OnSelectedItemChanged(e);
        IStswSelectionControl.SelectionChanged(this, new List<object>() { e.NewValue }, new List<object>() { e.OldValue });

        if (!_isSelectingPath)
            SelectedPath = e.NewValue is StswPathTreeItem fileItem ? fileItem.FullPath : null;
    }
    private bool _isSelectingPath = false;

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StswTreeViewItem listBoxItem)
        {
            listBoxItem.SetBinding(StswTreeViewItem.IsReadOnlyProperty, new Binding(nameof(IsReadOnly))
            {
                Source = this,
                Mode = BindingMode.OneWay
            });
        }
    }

    /// <summary>
    /// Handles the expansion of a <see cref="TreeViewItem"/> and asynchronously loads its child items if not already loaded.
    /// This method is triggered when a tree item is expanded and loads the directory contents if necessary.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnTreeViewItemExpanded(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is TreeViewItem item && item.DataContext is StswPathTreeItem directoryItem)
        {
            if (!directoryItem.IsLoaded)
                _ = LoadFolderItemsAsync(directoryItem);
        }
    }

    /// <summary>
    /// Loads folder contents asynchronously.
    /// This method retrieves the directories and files from a specified path and adds them to the parent item.
    /// </summary>
    /// <param name="parentItem">The parent folder item to load children for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadFolderItemsAsync(StswPathTreeItem parentItem)
    {
        if (parentItem.Children.Count == 1 && parentItem.Children[0] == null)
            parentItem.Children.Clear();

        // TODO - UnauthorizedAccessException
        var directories = await Task.Run(() => Directory.GetDirectories(parentItem.FullPath));
        var files = ShowFiles ? await Task.Run(() => Directory.GetFiles(parentItem.FullPath)) : [];

        foreach (var directory in directories)
            parentItem.Children.Add(new StswPathTreeItem(directory, StswPathType.OpenDirectory));

        foreach (var file in files)
            parentItem.Children.Add(new StswPathTreeItem(file, StswPathType.OpenFile));
    }

    /// <summary>
    /// Reloads the initial path into the file tree, populating the root items based on the initial path or logical drives.
    /// This method is invoked when the initial path is set or when files/folders should be reloaded.
    /// </summary>
    private void ReloadInitialPath()
    {
        var rootItems = new ObservableCollection<StswPathTreeItem>();

        if (string.IsNullOrEmpty(InitialPath))
        {
            foreach (var drive in Directory.GetLogicalDrives())
                rootItems.Add(new StswPathTreeItem(drive, StswPathType.OpenDirectory));
        }
        else
        {
            foreach (var directory in Directory.GetDirectories(InitialPath))
                rootItems.Add(new StswPathTreeItem(directory, StswPathType.OpenDirectory));

            if (ShowFiles)
                foreach (var file in Directory.GetFiles(InitialPath))
                    rootItems.Add(new StswPathTreeItem(file, StswPathType.OpenFile));
        }

        ItemsSource = rootItems;
    }

    /// <summary>
    /// Asynchronously selects an item in the tree based on the given file or folder path.
    /// This method expands the tree hierarchy and ensures the item is brought into view.
    /// </summary>
    /// <param name="fullPath">The full file or folder path to select.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SelectPathAsync(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return;

        var relativePath = fullPath;
        if (!string.IsNullOrEmpty(InitialPath) && fullPath.StartsWith(InitialPath, StringComparison.OrdinalIgnoreCase))
            relativePath = fullPath[InitialPath.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var rootItem = Items.OfType<StswPathTreeItem>().FirstOrDefault();
        if (rootItem == null)
            return;

        var currentItem = rootItem;
        for (var i = 1; i < pathParts.Length; i++)
        {
            if (!currentItem.IsLoaded)
                await LoadFolderItemsAsync(currentItem);

            var treeViewItem = FindTreeViewItemByPath(this, currentItem.FullPath);
            if (treeViewItem == null)
                return;

            treeViewItem.IsExpanded = true;
            await Task.Delay(50);

            currentItem = currentItem.Children.OfType<StswPathTreeItem>().FirstOrDefault(x => x.Name.Equals(pathParts[i], StringComparison.OrdinalIgnoreCase));
            if (currentItem == null)
                return;
        }

        var finalTreeViewItem = FindTreeViewItemByPath(this, currentItem.FullPath);
        if (finalTreeViewItem != null)
        {
            _isSelectingPath = true;

            finalTreeViewItem.IsSelected = true;
            finalTreeViewItem.BringIntoView();

            _isSelectingPath = false;
        }
    }

    /// <summary>
    /// Finds a <see cref="TreeViewItem"/> in the tree based on the specified full path.
    /// This method searches the tree recursively and returns the TreeViewItem corresponding to the provided path.
    /// </summary>
    /// <param name="container">The parent container to search within.</param>
    /// <param name="fullPath">The full path of the file or folder to find.</param>
    /// <returns>The <see cref="TreeViewItem"/> corresponding to the path, or <see langword="null"/> if not found.</returns>
    private TreeViewItem? FindTreeViewItemByPath(ItemsControl container, string fullPath)
    {
        foreach (var item in container.Items)
        {
            if (container.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
            {
                if (item is StswPathTreeItem fileItem && fileItem.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
                    return treeViewItem;

                var foundItem = FindTreeViewItemByPath(treeViewItem, fullPath);
                if (foundItem != null)
                    return foundItem;
            }
        }
        return null;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the initial path to be loaded into the file tree.
    /// If not set, logical drives will be displayed as the root items.
    /// This property defines the starting point for loading directories or files in the control.
    /// </summary>
    public string? InitialPath
    {
        get => (string?)GetValue(InitialPathProperty);
        set => SetValue(InitialPathProperty, value);
    }
    public static readonly DependencyProperty InitialPathProperty
        = DependencyProperty.Register(
            nameof(InitialPath),
            typeof(string),
            typeof(StswPathTree),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnInitialPathChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnInitialPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswPathTree stsw)
            return;

        stsw.ReloadInitialPath();
        // TODO - SelectedPath resets
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswPathTree)
        );

    /// <summary>
    /// Gets or sets the currently selected path in the control.
    /// This property represents the file or directory that is currently selected by the user.
    /// </summary>
    public string? SelectedPath
    {
        get => (string?)GetValue(SelectedPathProperty);
        set => SetValue(SelectedPathProperty, value);
    }
    public static readonly DependencyProperty SelectedPathProperty
        = DependencyProperty.Register(
            nameof(SelectedPath),
            typeof(string),
            typeof(StswPathTree),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedPathChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static async void OnSelectedPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswPathTree stsw)
            return;

        if (e.NewValue is string newPath)
            await stsw.SelectPathAsync(newPath);

        /// event for non MVVM programming
        stsw.SelectedPathChanged?.Invoke(stsw, new StswValueChangedEventArgs<string?>((string?)e.OldValue, (string?)e.NewValue));
    }

    /// <summary>
    /// Gets or sets a value indicating whether files should be shown in addition to folders in the tree.
    /// If true, both files and directories are shown in the control. If false, only directories are shown.
    /// </summary>
    public bool ShowFiles
    {
        get => (bool)GetValue(ShowFilesProperty);
        set => SetValue(ShowFilesProperty, value);
    }
    public static readonly DependencyProperty ShowFilesProperty
        = DependencyProperty.Register(
            nameof(ShowFiles),
            typeof(bool),
            typeof(StswPathTree),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnShowFilesChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnShowFilesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswPathTree stsw)
            return;

        stsw.ReloadInitialPath();
    }
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
            typeof(StswPathTree),
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
            typeof(StswPathTree),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
