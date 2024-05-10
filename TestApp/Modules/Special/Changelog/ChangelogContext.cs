using System.IO;

namespace TestApp;

public class ChangelogContext : StswObservableObject
{
    /// FilePath
    private string _filePath = Path.Combine(Directory.GetCurrentDirectory(), @"Resources\changelog_en.rtf");
    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }
}
