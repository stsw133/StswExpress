using System.IO;

namespace TestApp;

public class ChangelogContext : StswObservableObject
{
    /// FilePath
    private string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"Resources\changelog_en.rtf");
    public string FilePath
    {
        get => filePath;
        set => SetProperty(ref filePath, value);
    }
}
