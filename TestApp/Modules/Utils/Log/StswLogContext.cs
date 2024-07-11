namespace TestApp;
public class StswLogContext : StswObservableObject
{
    /// DirectoryPath
    public string DirectoryPath
    {
        get => _directoryPath;
        set => SetProperty(ref _directoryPath, value, () => StswLog.Config.LogDirectoryPath = value);
    }
    private string _directoryPath = StswLog.Config.LogDirectoryPath;
}
