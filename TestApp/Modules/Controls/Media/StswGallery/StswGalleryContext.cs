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
    public string? DirectoryPath
    {
        get => _directoryPath;
        set
        {
            SetProperty(ref _directoryPath, value);

            if (value != null)
                ItemsSource = [.. Directory.GetFiles(value)];
            else
                ItemsSource = [];
        }
    }
    private string? _directoryPath;

    /// ItemsSource
    public List<string> ItemsSource
    {
        get => _itemsSource;
        set => SetProperty(ref _itemsSource, value);
    }
    private List<string> _itemsSource = [];

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;
}
