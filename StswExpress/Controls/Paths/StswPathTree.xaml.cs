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
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a custom file tree control that displays folders and files in a hierarchical structure.
/// This control allows for folder expansion and item selection with support for animations and custom paths.
/// </summary>
[ContentProperty(nameof(SelectedPath))]
public class StswPathTree : TreeView, IStswCornerControl, IStswSelectionControl
{
    public StswPathTree()
    {
        AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnTreeViewItemExpanded));
    }
    static StswPathTree()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPathTree), new FrameworkPropertyMetadata(typeof(StswPathTree)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswPathTree), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the selected path in the control changes.
    /// </summary>
    public event EventHandler? SelectedPathChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        OnInitialPathChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Handles item selection changes and animations.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
    {
        base.OnSelectedItemChanged(e);
        IStswSelectionControl.SelectionChanged(this, new List<object>() { e.NewValue }, new List<object>() { e.OldValue });

        if (!_isSelectingPath)
            SelectedPath = e.NewValue is StswPathTreeItem fileItem ? fileItem.FullPath : null;
    }
    private bool _isSelectingPath = false;

    /// <summary>
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Occurs when the ItemTemplate property value changes.
    /// </summary>
    /// <param name="oldItemTemplate">The old value of the ItemTemplate property.</param>
    /// <param name="newItemTemplate">The new value of the ItemTemplate property.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <summary>
    /// Handles the expansion of a <see cref="TreeViewItem"/> and asynchronously loads its child items if not already loaded.
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
            parentItem.Children.Add(new StswPathTreeItem(directory, StswPathType.Directory));

        foreach (var file in files)
            parentItem.Children.Add(new StswPathTreeItem(file, StswPathType.File));
    }

    /// <summary>
    /// Reloads the initial path into the file tree, populating the root items based on the initial path or logical drives.
    /// </summary>
    private void ReloadInitialPath()
    {
        var rootItems = new ObservableCollection<StswPathTreeItem>();

        if (string.IsNullOrEmpty(InitialPath))
        {
            foreach (var drive in Directory.GetLogicalDrives())
                rootItems.Add(new StswPathTreeItem(drive, StswPathType.Directory));
        }
        else
        {
            foreach (var directory in Directory.GetDirectories(InitialPath))
                rootItems.Add(new StswPathTreeItem(directory, StswPathType.Directory));

            if (ShowFiles)
                foreach (var file in Directory.GetFiles(InitialPath))
                    rootItems.Add(new StswPathTreeItem(file, StswPathType.File));
        }

        ItemsSource = rootItems;
    }

    /// <summary>
    /// Asynchronously selects an item in the tree based on the given file or folder path.
    /// Expands the tree hierarchy and ensures the item is brought into view.
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
        if (obj is StswPathTree stsw)
        {
            stsw.ReloadInitialPath();
            // TODO - SelectedPath resets
        }
    }

    /// <summary>
    /// Gets or sets the currently selected path in the control.
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
        if (obj is StswPathTree stsw)
        {
            if (e.NewValue is string newPath)
                await stsw.SelectPathAsync(newPath);

            stsw.SelectedPathChanged?.Invoke(stsw, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether files should be shown in addition to folders in the tree.
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
        if (obj is StswPathTree stsw)
        {
            stsw.ReloadInitialPath();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control uses selection items that implement the <see cref="IStswSelectionItem"/> interface.
    /// </summary>
    public bool UsesSelectionItems
    {
        get => (bool)GetValue(UsesSelectionItemsProperty);
        set => SetValue(UsesSelectionItemsProperty, value);
    }
    public static readonly DependencyProperty UsesSelectionItemsProperty
        = DependencyProperty.Register(
            nameof(UsesSelectionItems),
            typeof(bool),
            typeof(StswPathTree)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
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

/// <summary>
/// Represents a file or folder item in the <see cref="StswPathTree"/> control.
/// </summary>
internal class StswPathTreeItem : StswObservableObject
{
    public StswPathTreeItem(string fullPath, StswPathType type)
    {
        FullPath = fullPath;
        Type = type;

        if (type == StswPathType.Directory)
            Children.Add(null);

        LoadIconAsync();
    }

    /// <summary>
    /// Gets or sets the full path of the file or folder.
    /// </summary>
    public string FullPath { get; set; }

    /// <summary>
    /// Gets or sets the icon representing the file or folder.
    /// </summary>
    public ImageSource? Icon { get; set; }

    /// <summary>
    /// Gets or sets the name of the file or folder (derived from the full path).
    /// </summary>
    public string? Name => Path.GetFileName(FullPath) is string fileName && !string.IsNullOrEmpty(fileName) ? fileName : FullPath;

    /// <summary>
    /// Gets or sets the type of the item (file or directory).
    /// </summary>
    public StswPathType Type { get; set; }

    /// <summary>
    /// Gets or sets the collection of child items for this folder.
    /// </summary>
    public ObservableCollection<StswPathTreeItem?> Children { get; set; } = [];

    /// <summary>
    /// Asynchronously loads the associated icon for the file or folder and updates the <see cref="Icon"/> property.
    /// </summary>
    private async void LoadIconAsync()
    {
        var icon = await Task.Run(() => StswFn.ExtractAssociatedIcon(FullPath));
        if (icon != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Icon = icon.ToImageSource();
                OnPropertyChanged(nameof(Icon));
            });
        }
    }

    /// <summary>
    /// Gets a value indicating whether the folder's children have been loaded.
    /// </summary>
    public bool IsLoaded => Children.Count != 1 || Children[0] != null;
}
