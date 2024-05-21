namespace TestApp;
public class ChangelogModel
{
    public string Version { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public ChangelogType Type { get; set; }
}
