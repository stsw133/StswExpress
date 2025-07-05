using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a file or folder item in the <see cref="StswPathTree"/> control.
/// </summary>
[Stsw("0.13.0", Changes = StswPlannedChanges.None)]
internal class StswPathTreeItem : StswObservableObject
{
    public StswPathTreeItem(string fullPath, StswPathType type)
    {
        FullPath = fullPath;
        Type = type;

        if (type == StswPathType.OpenDirectory)
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
    public string Name => Path.GetFileName(FullPath) is string fileName && !string.IsNullOrEmpty(fileName) ? fileName : FullPath;

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
        var icon = await Task.Run(() => StswFnUI.ExtractAssociatedIcon(FullPath));
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
