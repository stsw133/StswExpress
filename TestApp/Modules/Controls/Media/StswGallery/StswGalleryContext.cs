using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswGalleryContext : ControlsContext
{
    public StswCommand SelectItemsSourceCommand => new(SelectItemsSource);

    public override void SetDefaults()
    {
        base.SetDefaults();

        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    #region Commands & methods
    /// Command: select ItemsSource
    public void SelectItemsSource()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            DirectoryPath = dialog.SelectedPath;
    }
    #endregion

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
    private Orientation orientation;
    public Orientation Orientation
    {
        get => orientation;
        set => SetProperty(ref orientation, value);
    }
}
