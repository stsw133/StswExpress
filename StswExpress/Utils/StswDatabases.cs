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
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string _name = string.Empty;

    /// Type
    public StswDatabaseType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    private StswDatabaseType _type = default;

    /// Server
    public string Server
    {
        get => _server;
        set => SetProperty(ref _server, value);
    }
    private string _server = string.Empty;

    /// Port
    public int? Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }
    private int? _port;

    /// Database
    public string Database
    {
        get => _database;
        set => SetProperty(ref _database, value);
    }
    private string _database = string.Empty;

    /// Login
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }
    private string _login = string.Empty;

    /// Password
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    private string _password = string.Empty;

    /// Version
    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }
    private string _version = string.Empty;
    
    /// <summary>
    /// Puts together all the model's properties to create a database connection in the form of a string.
    /// </summary>
    public string GetConnString() => Type switch
    {
        StswDatabaseType.MSSQL => $"Server={Server}{(Port > 0 ? $",{Port}" : "")};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
        StswDatabaseType.MySQL => $"Server={Server};{(Port > 0 ? $"Port={Port}" : string.Empty)};Database={Database};Uid={Login};Pwd={Password};Application Name={StswFn.AppName()};",
        StswDatabaseType.PostgreSQL => $"Server={Server};Port={Port ?? 5432};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
        _ => throw new Exception("This type of database management system is not supported!")
    };
}