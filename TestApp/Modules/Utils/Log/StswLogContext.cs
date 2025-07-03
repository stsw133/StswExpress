namespace TestApp;
public partial class StswLogContext : StswObservableObject
{
    //public string DirectoryPath
    //{
    //    get => _directoryPath;
    //    set => SetProperty(ref _directoryPath, value, () => StswLog.Config.LogDirectoryPath = value);
    //}
    [StswObservableProperty(CallbackMethod = "() => StswLog.Config.LogDirectoryPath = value")] string _directoryPath = StswLog.Config.LogDirectoryPath;
}
