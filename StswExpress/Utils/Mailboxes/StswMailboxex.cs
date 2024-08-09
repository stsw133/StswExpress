using System;
using System.Collections.Generic;
using System.IO;

namespace StswExpress;
/// <summary>
/// Provides functionality for managing email configurations and methods to import and export them.
/// </summary>
public static class StswMailboxex
{
    /// <summary>
    /// Gets or sets the default instance of email configuration that is currently in use by the application in case only a single one is needed.
    /// </summary>
    public static StswMailboxModel? Default { get; set; }

    /// <summary>
    /// Gets or sets the location of the file where email configurations are stored.
    /// </summary>
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "mailboxes.stsw");

    /// <summary>
    /// Writes email configurations to a file specified by <see cref="FilePath"/>.
    /// </summary>
    /// <param name="collection">The collection of <see cref="StswMailboxModel"/> objects representing the email configurations to be exported.</param>
    public static void ExportList(IEnumerable<StswMailboxModel> collection)
    {
        using var stream = new StreamWriter(FilePath);
        foreach (var item in collection)
            stream.WriteLine(StswSecurity.Encrypt(item.Name ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(item.Host ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(item.Port.ToString() ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(item.Address ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(item.Username ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(item.Password ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(item.Domain ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(item.EnableSSL.ToString() ?? string.Empty)
                );
    }

    /// <summary>
    /// Reads email configurations from a file specified by <see cref="FilePath"/> and saves them in a collection.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="StswMailboxModel"/> objects representing the imported email configurations.</returns>
    public static IEnumerable<StswMailboxModel> ImportList()
    {
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
                yield return new StswMailboxModel()
                {
                    Name = StswSecurity.Decrypt(data[0]),
                    Host = StswSecurity.Decrypt(data[1]),
                    Port = StswSecurity.Decrypt(data[2]) is string s1 && !string.IsNullOrEmpty(s1) ? Convert.ToInt32(s1) : 0,
                    Address = StswSecurity.Decrypt(data[3]),
                    Username = StswSecurity.Decrypt(data[4]),
                    Password = StswSecurity.Decrypt(data[5]),
                    Domain = StswSecurity.Decrypt(data[6]) is string s2 && !string.IsNullOrEmpty(s2) ? s2 : null,
                    EnableSSL = StswSecurity.Decrypt(data[7]) is string s3 && !string.IsNullOrEmpty(s3) && Convert.ToBoolean(s3)
                };
            }
        }
    }
}
