namespace StswExpress.Commons;
/// <summary>
/// Represents a model for database connection, including methods for building connection strings and opening database connections.
/// </summary>
[StswInfo(null, Changes = StswPlannedChanges.ChangeName)]
public partial class StswDatabaseModel : StswObservableObject
{
    public StswDatabaseModel() { }
    public StswDatabaseModel(string? server = null, string? database = null, string? login = null, string? password = null) : this(server, null, database, login, password) { }
    public StswDatabaseModel(string? server = null, int? port = null, string? database = null, string? login = null, string? password = null)
    {
        Server = server;
        Port = port;
        Database = database;
        Login = login;
        Password = password;
    }

    /// <summary>
    /// Gets or sets the name of the database connection.
    /// </summary>
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string? _name;

    /// <summary>
    /// Gets or sets the type of the database.
    /// </summary>
    public StswDatabaseType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    private StswDatabaseType _type = StswDatabaseType.MSSQL;

    /// <summary>
    /// Gets or sets the server address of the database.
    /// </summary>
    public string? Server
    {
        get => _server;
        set => SetProperty(ref _server, value);
    }
    private string? _server;

    /// <summary>
    /// Gets or sets the port number of the database.
    /// </summary>
    public int? Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }
    private int? _port;

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string? Database
    {
        get => _database;
        set => SetProperty(ref _database, value);
    }
    private string? _database;

    /// <summary>
    /// Gets or sets the login name for the database.
    /// </summary>
    public string? Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }
    private string? _login;

    /// <summary>
    /// Gets or sets the password for the database.
    /// </summary>
    public string? Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    private string? _password;

    /// <summary>
    /// Gets or sets the version of the database.
    /// </summary>
    public string? Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }
    private string? _version;

    /// <summary>
    /// Gets or sets the default timeout for all commands associated with the database.
    /// </summary>
    public int? DefaultTimeout
    {
        get => _defaultTimeout;
        set => SetProperty(ref _defaultTimeout, value);
    }
    private int? _defaultTimeout;

    /// <summary>
    /// Gets or sets the encrypt mode for the connection.
    /// </summary>
    public bool Encrypt
    {
        get => _encrypt;
        set => SetProperty(ref _encrypt, value);
    }
    private bool _encrypt;

    /// <summary>
    /// Gets or sets whether Windows Authentication should be used (Integrated Security).
    /// </summary>
    public bool UseIntegratedSecurity
    {
        get => _useIntegratedSecurity;
        set => SetProperty(ref _useIntegratedSecurity, value);
    }
    private bool _useIntegratedSecurity;

    /// <summary>
    /// Constructs the connection string based on the model's properties.
    /// </summary>
    /// <returns>The database connection string.</returns>
    public string GetConnString()
    {
        if (string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(Database))
            throw new InvalidOperationException("Server and Database must be specified.");

        if (!UseIntegratedSecurity && (string.IsNullOrEmpty(Login) || Password == null))
            throw new InvalidOperationException("Login and Password must be specified when not using integrated security.");

        var appName = $"Application Name={StswFn.AppName()};";

        return Type switch
        {
            StswDatabaseType.MSSQL => UseIntegratedSecurity
                ? $"Server={Server}{(Port.HasValue ? $",{Port}" : "")};Database={Database};Integrated Security=True;Encrypt={Encrypt};{appName}"
                : $"Server={Server}{(Port.HasValue ? $",{Port}" : "")};Database={Database};User Id={Login};Password={Password};Encrypt={Encrypt};{appName}",

            StswDatabaseType.MySQL => UseIntegratedSecurity
                ? throw new NotSupportedException("Integrated security is not supported for MySQL.")
                : $"Server={Server}{(Port.HasValue ? $";Port={Port}" : "")};Database={Database};Uid={Login};Pwd={Password};Encrypt={Encrypt};{appName}",

            StswDatabaseType.PostgreSQL => UseIntegratedSecurity
                ? throw new NotSupportedException("Integrated security is not supported for PostgreSQL.")
                : $"Host={Server}{(Port.HasValue ? $";Port={Port}" : "")};Database={Database};User Id={Login};Password={Password};Encrypt={Encrypt};{appName}",

            _ => throw new NotSupportedException("This type of database management system is not supported!")
        };
    }
}
