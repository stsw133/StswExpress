using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TestApp;
public partial class GalleryContext : StswObservableObject
{
    [StswCommand] void ChangeNavigationMode()
    {
        if (StswFnUI.FindVisualChild<StswNavigation>(StswApp.StswWindow) is StswNavigation navi)
        {
            if (!StswApp.StswWindow.Fullscreen && navi.TabStripMode != StswCompactibility.Collapsed)
                StswApp.StswWindow.Fullscreen = true;
            else if (StswApp.StswWindow.Fullscreen && navi.TabStripMode == StswCompactibility.Collapsed)
                StswApp.StswWindow.Fullscreen = false;

            navi.BorderThickness = navi.TabStripMode == StswCompactibility.Collapsed ? new(0, 1, 0, 0) : new(0);
            navi.TabStripMode = navi.TabStripMode == StswCompactibility.Collapsed ? StswCompactibility.Compact : StswCompactibility.Collapsed;
        }
    }
    [StswCommand] void SelectDirectory()
    {
        var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
            SelectedDirectory = dialog.SelectedPath;
    }
    [StswCommand] void LoadDirectory()
    {
        if (Directory.Exists(SelectedDirectory))
            SelectedFile = Directory.GetFiles(SelectedDirectory).First();
    }
    [StswCommand] void NextFile(string parameter)
    {
        if (Directory.Exists(SelectedDirectory) && !string.IsNullOrEmpty(SelectedFile) && int.TryParse(parameter, out var step))
        {
            var files = Directory.GetFiles(SelectedDirectory).ToList();
            if (files.Count > 0)
            {
                int newIndex = files.IndexOf(SelectedFile);
                if (IsLoopingEnabled)
                    newIndex = (newIndex + step % files.Count + files.Count) % files.Count;
                else if (newIndex + step is int i && i.Between(0, files.Count - 1))
                    newIndex = i;

                SelectedFile = files[newIndex];
            }
        }
    }

    [StswObservableProperty] bool _isLoopingEnabled;
    [StswObservableProperty] string? _selectedFile;
    [StswObservableProperty] string? _selectedDirectory;
    partial void OnSelectedDirectoryChanged(string? oldValue, string? newValue) => LoadDirectory();
}
