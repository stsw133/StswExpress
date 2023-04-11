using System;
using System.IO;

namespace StswExpress;

/// <summary>
/// Provides functionality for managing database connections and methods to import and export them.
/// </summary>
public static class StswDatabase
{
    /// <summary>
    /// The dictionary that contains all declared database connections for application.
    /// </summary>
    public static StswDictionary<string, StswDatabaseModel> AllDatabases { get; set; } = new();

    /// <summary>
    /// Default instance of database connection (that is currently in use by application). 
    /// </summary>
    public static StswDatabaseModel? CurrentDatabase { get; set; }

    /// <summary>
    /// Specifies the location of the file where database connections are stored.
    /// </summary>
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "databases.stsw");

    /// <summary>
    /// Reads database connections from a file specified by <see cref="FilePath"/> and saves them in <see cref="AllMailboxes"/>.
    /// </summary>
    public static void ImportDatabases()
    {
        AllDatabases.Clear();

        if (!File.Exists(FilePath))
        {
            (new FileInfo(FilePath))?.Directory?.Create();
            File.Create(FilePath).Close();
        }

        using var stream = new StreamReader(FilePath);
        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine();
            if (line != null)
            {
                var data = line.Split('|');
                AllDatabases.Add(StswSecurity.Decrypt(data[0]), new StswDatabaseModel()
                {
                    Server = StswSecurity.Decrypt(data[1]),
                    Port = Convert.ToInt32(StswSecurity.Decrypt(data[2])),
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
    public static void ExportDatabases()
    {
        using var stream = new StreamWriter(FilePath);
        foreach (var db in AllDatabases)
            stream.WriteLine(StswSecurity.Encrypt(db.Key)
                    + "|" + StswSecurity.Encrypt(db.Value.Server)
                    + "|" + StswSecurity.Encrypt(db.Value.Port.ToString())
                    + "|" + StswSecurity.Encrypt(db.Value.Database)
                    + "|" + StswSecurity.Encrypt(db.Value.Login)
                    + "|" + StswSecurity.Encrypt(db.Value.Password)
                );
    }
}

/// <summary>
/// Represents a model for database connection.
/// </summary>
public class StswDatabaseModel
{
    /// <summary>
    /// Supported database systems.
    /// </summary>
    public enum Types
    {
        MSSQL,
        PostgreSQL
    }

    public Types Type { get; set; } = default;
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string Database { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Puts together all the model's properties to create a database connection string in the form of a string.
    /// </summary>
    public string? GetConnString()
    {
        return Type switch
        {
            Types.MSSQL => $"Server={Server}{(Port > 0 ? $",Port={Port}" : string.Empty)};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
            Types.PostgreSQL => $"Server={Server};Port={Port};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
            _ => null
        };
    }
}