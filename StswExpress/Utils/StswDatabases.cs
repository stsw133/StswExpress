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
    /// The dictionary that contains all declared database connections for application.
    /// </summary>
    public static ObservableCollection<StswDatabaseModel> List { get; set; } = new();

    /// <summary>
    /// Default instance of database connection (that is currently in use by application). 
    /// </summary>
    public static StswDatabaseModel? Current { get; set; }

    /// <summary>
    /// Specifies the location of the file where database connections are stored.
    /// </summary>
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "databases.stsw");

    /// <summary>
    /// Reads database connections from a file specified by <see cref="FilePath"/> and saves them in <see cref="List"/>.
    /// </summary>
    public static void ImportList()
    {
        List.Clear();

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
                List.Add(new()
                {
                    Name = StswSecurity.Decrypt(data[0]),
                    Server = StswSecurity.Decrypt(data[1]),
                    Port = StswSecurity.Decrypt(data[2]) is string s && !string.IsNullOrEmpty(s) ? Convert.ToInt32(s) : null,
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
        foreach (var db in List)
            stream.WriteLine(StswSecurity.Encrypt(db.Name)
                    + "|" + StswSecurity.Encrypt(db.Server)
                    + "|" + StswSecurity.Encrypt(db.Port?.ToString() ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(db.Database)
                    + "|" + StswSecurity.Encrypt(db.Login)
                    + "|" + StswSecurity.Encrypt(db.Password)
                );
    }
}

/// <summary>
/// Represents a model for database connection.
/// </summary>
public class StswDatabaseModel : StswObservableObject
{
    /// Name
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }
    private string name = string.Empty;

    /// Type
    public StswDatabaseType Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }
    private StswDatabaseType type = default;

    /// Server
    public string Server
    {
        get => server;
        set => SetProperty(ref server, value);
    }
    private string server = string.Empty;

    /// Port
    public int? Port
    {
        get => port;
        set => SetProperty(ref port, value);
    }
    private int? port;

    /// Database
    public string Database
    {
        get => database;
        set => SetProperty(ref database, value);
    }
    private string database = string.Empty;

    /// Login
    public string Login
    {
        get => login;
        set => SetProperty(ref login, value);
    }
    private string login = string.Empty;

    /// Password
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }
    private string password = string.Empty;

    /// Version
    public string Version
    {
        get => version;
        set => SetProperty(ref version, value);
    }
    private string version = string.Empty;
    
    /// <summary>
    /// Puts together all the model's properties to create a database connection in the form of a string.
    /// </summary>
    public string? GetConnString() => Type switch
    {
        StswDatabaseType.MSSQL => $"Server={Server}{(Port > 0 ? $",{Port}" : "")};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
        StswDatabaseType.MySQL => $"Server={Server};{(Port > 0 ? $"Port={Port}" : string.Empty)};Database={Database};Uid={Login};Pwd={Password};Application Name={StswFn.AppName()};",
        StswDatabaseType.PostgreSQL => $"Server={Server};Port={Port ?? 5432};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
        _ => null
    };
}