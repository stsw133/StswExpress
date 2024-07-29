using System;
using System.Collections.ObjectModel;
using System.IO;

namespace StswExpress;
/// <summary>
/// Provides functionality for managing database connections and methods to import and export them.
/// </summary>
public static class StswDatabases
{
    /// <summary>
    /// The collection that contains all declared database connections for the application.
    /// </summary>
    public static ObservableCollection<StswDatabaseModel> Collection { get; set; } = [];

    /// <summary>
    /// Default instance of the database connection that is currently in use by the application. 
    /// </summary>
    public static StswDatabaseModel? Current { get; set; }

    /// <summary>
    /// Specifies the location of the file where database connections are stored.
    /// </summary>
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "databases.stsw");

    /// <summary>
    /// Reads database connections from a file specified by <see cref="FilePath"/> and saves them in <see cref="Collection"/>.
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
                Collection.Add(new()
                {
                    Name = StswSecurity.Decrypt(data[0]),
                    Server = StswSecurity.Decrypt(data[1]),
                    Port = int.TryParse(StswSecurity.Decrypt(data[2]), out var port) ? port : null,
                    Database = StswSecurity.Decrypt(data[3]),
                    Login = StswSecurity.Decrypt(data[4]),
                    Password = StswSecurity.Decrypt(data[5])
                });
            }
        }
    }

    /// <summary>
    /// Writes database connections to a file specified by <see cref="FilePath"/>.
    /// </summary>
    public static void ExportList()
    {
        using var stream = new StreamWriter(FilePath);
        foreach (var db in Collection)
            stream.WriteLine(string.Join('|', 
                    StswSecurity.Encrypt(db.Name ?? string.Empty),
                    StswSecurity.Encrypt(db.Server ?? string.Empty),
                    StswSecurity.Encrypt(db.Port?.ToString() ?? string.Empty),
                    StswSecurity.Encrypt(db.Database ?? string.Empty),
                    StswSecurity.Encrypt(db.Login ?? string.Empty),
                    StswSecurity.Encrypt(db.Password ?? string.Empty)
                ));
    }
}
