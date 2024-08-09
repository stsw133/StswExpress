using System;
using System.Collections.Generic;
using System.IO;

namespace StswExpress;
/// <summary>
/// Provides functionality for managing database connections and methods to import and export them.
/// </summary>
public static class StswDatabases
{
    /// <summary>
    /// Gets or sets a value indicating whether to always make less space in the query.
    /// </summary>
    public static bool AlwaysMakeLessSpaceQuery { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to always return if in designer mode.
    /// </summary>
    public static bool AlwaysReturnIfInDesignerMode { get; set; } = true;

    /// <summary>
    /// Gets or sets the default instance of the database connection that is currently in use by the application in case only a single one is needed.
    /// </summary>
    public static StswDatabaseModel? Default { get; set; }

    /// <summary>
    /// Gets or sets the location of the file where database connections are stored.
    /// </summary>
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "databases.stsw");

    /// <summary>
    /// Writes database connections to a file specified by <see cref="FilePath"/>.
    /// </summary>
    /// <param name="collection">The collection of <see cref="StswDatabaseModel"/> objects representing the database connections to be exported.</param>
    public static void ExportList(IEnumerable<StswDatabaseModel> collection)
    {
        using var stream = new StreamWriter(FilePath);
        foreach (var item in collection)
            stream.WriteLine(string.Join('|', 
                    StswSecurity.Encrypt(item.Name ?? string.Empty),
                    StswSecurity.Encrypt(item.Server ?? string.Empty),
                    StswSecurity.Encrypt(item.Port?.ToString() ?? string.Empty),
                    StswSecurity.Encrypt(item.Database ?? string.Empty),
                    StswSecurity.Encrypt(item.Login ?? string.Empty),
                    StswSecurity.Encrypt(item.Password ?? string.Empty)
                ));
    }

    /// <summary>
    /// Reads database connections from a file specified by <see cref="FilePath"/> and saves them in a collection.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="StswDatabaseModel"/> objects representing the imported database connections.</returns>
    public static IEnumerable<StswDatabaseModel> ImportList()
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
                yield return new()
                {
                    Name = StswSecurity.Decrypt(data[0]),
                    Server = StswSecurity.Decrypt(data[1]),
                    Port = int.TryParse(StswSecurity.Decrypt(data[2]), out var port) ? port : null,
                    Database = StswSecurity.Decrypt(data[3]),
                    Login = StswSecurity.Decrypt(data[4]),
                    Password = StswSecurity.Decrypt(data[5])
                };
            }
        }
    }
}
