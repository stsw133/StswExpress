namespace TestApp;
public partial class StswLogContext : StswObservableObject
{
    [StswObservableProperty] string _directoryPath = StswLog.Config.LogDirectoryPath;
    partial void OnDirectoryPathChanged(string oldValue, string newValue) => StswLog.Config.LogDirectoryPath = newValue;
}
