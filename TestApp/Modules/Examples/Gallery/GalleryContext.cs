using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TestApp;

public class GalleryContext : StswObservableObject
{
    public StswCommand ChangeNavigationModeCommand { get; set; }
    public StswCommand SelectDirectoryCommand { get; set; }
    public StswCommand<string?> NextFileCommand { get; set; }

    public GalleryContext()
    {
        ChangeNavigationModeCommand = new(ChangeNavigationMode);
        SelectDirectoryCommand = new(SelectDirectory);
        NextFileCommand = new(NextFile);
    }

    #region Commands & methods
    /// ChangeNavigationMode
    private void ChangeNavigationMode()
    {
        if (StswFn.FindVisualChild<StswNavigation>(StswApp.StswWindow) is StswNavigation navi)
        {
            if (!StswApp.StswWindow.Fullscreen && navi.TabStripMode != StswCompactibility.Collapsed)
                StswApp.StswWindow.Fullscreen = true;
            else if (StswApp.StswWindow.Fullscreen && navi.TabStripMode == StswCompactibility.Collapsed)
                StswApp.StswWindow.Fullscreen = false;

            navi.BorderThickness = navi.TabStripMode == StswCompactibility.Collapsed ? new(0, 1, 0, 0) : new(0);
            navi.TabStripMode = navi.TabStripMode == StswCompactibility.Collapsed ? StswCompactibility.Compact : StswCompactibility.Collapsed;
        }
    }

    /// SelectDirectory
    private void SelectDirectory()
    {
        var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
            SelectedDirectory = dialog.SelectedPath;
    }

    /// LoadDirectory
    private void LoadDirectory()
    {
        if (Directory.Exists(SelectedDirectory))
            SelectedFile = Directory.GetFiles(SelectedDirectory).First();
    }

    /// NextFile
    private void NextFile(string? parameter)
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
    #endregion

    /// IsLoopingEnabled
    public bool IsLoopingEnabled
    {
        get => _isLoopingEnabled;
        set => SetProperty(ref _isLoopingEnabled, value);
    }
    private bool _isLoopingEnabled;

    /// SelectedDirectory
    public string? SelectedDirectory
    {
        get => _selectedDirectory;
        set => SetProperty(ref _selectedDirectory, value, LoadDirectory);
    }
    private string? _selectedDirectory;

    /// SelectedFile
    public string? SelectedFile
    {
        get => _selectedFile;
        set => SetProperty(ref _selectedFile, value);
    }
    private string? _selectedFile;
}
