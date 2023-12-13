using System;
using System.Windows.Input;

namespace TestApp;

public class StswMediaPlayerContext : ControlsContext
{
    public ICommand SelectItemSourceCommand { get; set; }

    public StswMediaPlayerContext()
    {
        SelectItemSourceCommand = new StswCommand(SelectItemSource);
    }

    #region Commands
    /// Command: select Source
    public void SelectItemSource()
    {
        var dialog = new System.Windows.Forms.OpenFileDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            FilePath = dialog.FileName;
    }
    #endregion

    #region Properties
    /// FilePath
    private string? filePath;
    public string? FilePath
    {
        get => filePath;
        set
        {
            SetProperty(ref filePath, value);

            if (value != null)
                Source = new Uri(value);
            else
                Source = null;
        }
    }

    /// Source
    private Uri? source;
    public Uri? Source
    {
        get => source;
        set => SetProperty(ref source, value);
    }
    #endregion
}
