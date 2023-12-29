using System;

namespace TestApp;

public class StswMediaElementContext : ControlsContext
{
    public StswCommand SelectItemSourceCommand => new(SelectItemSource);

    #region Commands & methods
    /// Command: select Source
    public void SelectItemSource()
    {
        var dialog = new System.Windows.Forms.OpenFileDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            FilePath = dialog.FileName;
    }
    #endregion

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
}
