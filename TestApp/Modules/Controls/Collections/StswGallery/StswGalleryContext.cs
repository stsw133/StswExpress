using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace TestApp;

public class StswGalleryContext : ControlsContext
{
    public ICommand SelectItemsSourceCommand { get; set; }

    public StswGalleryContext()
    {
        SelectItemsSourceCommand = new StswCommand(SelectItemsSource);
    }

    #region Commands
    /// Command: select ItemsSource
    public void SelectItemsSource()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            DirectoryPath = dialog.SelectedPath;
    }
    #endregion

    #region Properties
    /// DirectoryPath
    private string? directoryPath;
    public string? DirectoryPath
    {
        get => directoryPath;
        set
        {
            SetProperty(ref directoryPath, value);

            if (value != null)
                ItemsSource = Directory.GetFiles(value).ToList();
            else
                ItemsSource = new();
        }
    }

    /// ItemsSource
    private List<string> itemsSource = new();
    public List<string> ItemsSource
    {
        get => itemsSource;
        set => SetProperty(ref itemsSource, value);
    }

    /// Orientation
    private Orientation orientation = Orientation.Horizontal;
    public Orientation Orientation
    {
        get => orientation;
        set => SetProperty(ref orientation, value);
    }
    #endregion
}
