using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestApp;

public class ChangelogContext : StswObservableObject
{
    public ChangelogContext()
    {
        foreach (var changelog in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @"Resources\changelogs")).OrderBy(x => x))
        {
            var version = Path.GetFileNameWithoutExtension(changelog).Split('_')[1];
            var major = string.Empty;
            var minor = string.Empty;
            var patch = string.Empty;

            if (version.Split('.') is string[] versionParts && versionParts.Length == 3)
            {
                major = versionParts[0];
                minor = versionParts[1];
                patch = versionParts[2];
            }

            Changelogs.Add(new()
            {
                Version = version,
                FilePath = changelog,
                Type = minor == "0" ? ChangelogType.Major : patch == "0" ? ChangelogType.Minor : ChangelogType.Patch
            });
        }

        SelectedChangelog = Changelogs.Last();
    }

    /// Changelogs
    public List<ChangelogModel> Changelogs
    {
        get => _changelogs;
        set => SetProperty(ref _changelogs, value);
    }
    private List<ChangelogModel> _changelogs = [];
    
    /// SelectedChangelog
    public ChangelogModel? SelectedChangelog
    {
        get => _selectedChangelog;
        set => SetProperty(ref _selectedChangelog, value);
    }
    private ChangelogModel? _selectedChangelog;
}
