using System;
using System.IO;
using System.Collections.ObjectModel;

namespace StswExpress;
/// <summary>
/// Provides functionality for managing email configurations and methods to import and export them.
/// </summary>
public static class StswMailboxex
{
    /// <summary>
    /// The dictionary that contains all declared email configurations for application.
    /// </summary>
    public static ObservableCollection<StswMailboxModel> Collection { get; set; } = [];

    /// <summary>
    /// Default instance of email configuration (that is currently in use by application). 
    /// </summary>
    public static StswMailboxModel? Current { get; set; }

    /// <summary>
    /// Specifies the location of the file where email configurations are stored.
    /// </summary>
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "mailboxes.stsw");

    /// <summary>
    /// Reads email configurations from a file specified by <see cref="FilePath"/> and saves them in <see cref="Collection"/>.
    /// </summary>
    public static void ImportList()
    {
        Collection.Clear();

        if (!File.Exists(FilePath))
        {
            new FileInfo(FilePath)?.Directory?.Create();
            File.Create(FilePath).Close();
        }

        using var stream = new StreamReader(FilePath);
        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine();
            if (line != null)
            {
                var data = line.Split('|');
                Collection.Add(new StswMailboxModel()
                {
                    Name = StswSecurity.Decrypt(data[0]),
                    Host = StswSecurity.Decrypt(data[1]),
                    Port = StswSecurity.Decrypt(data[2]) is string s1 && !string.IsNullOrEmpty(s1) ? Convert.ToInt32(s1) : 0,
                    Address = StswSecurity.Decrypt(data[3]),
                    Username = StswSecurity.Decrypt(data[4]),
                    Password = StswSecurity.Decrypt(data[5]),
                    Domain = StswSecurity.Decrypt(data[6]) is string s2 && !string.IsNullOrEmpty(s2) ? s2 : null,
                    EnableSSL = StswSecurity.Decrypt(data[7]) is string s3 && !string.IsNullOrEmpty(s3) && Convert.ToBoolean(s3)
                });
            }
        }
    }

    /// <summary>
    /// Writes email configurations to a file specified by <see cref="FilePath"/>.
    /// </summary>
    public static void ExportList()
    {
        using var stream = new StreamWriter(FilePath);
        foreach (var mc in Collection)
            stream.WriteLine(StswSecurity.Encrypt(mc.Name ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.Host ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.Port.ToString() ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.Address ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.Username ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.Password ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.Domain ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.EnableSSL.ToString() ?? string.Empty)
                );
    }
}
