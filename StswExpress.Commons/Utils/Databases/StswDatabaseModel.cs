using Microsoft.Data.SqlClient;

namespace StswExpress.Commons;
/// <summary>
/// Represents a model for database connection, including methods for building connection strings and opening database connections.
/// </summary>
//TODO - remove or base it on SqlConnectionStringBuilder
public partial class StswDatabaseModel : StswObservableObject
{
    public StswDatabaseModel() { }
    public StswDatabaseModel(string? server = null, int? port = null, string? database = null, string? login = null, string? password = null)
    {
        Server = server;
        Port = port;
        Database = database;
        Login = login;
        Password = password;
    }
    public StswDatabaseModel(SqlConnection sqlConn)
    {
        Server = sqlConn.DataSource;
        Database = sqlConn.Database;

        var builder = new SqlConnectionStringBuilder(sqlConn.ConnectionString);
        Login = builder.UserID;
        Password = builder.Password;
    }

    /// <summary>
    /// Gets or sets the name of the database connection.
    /// </summary>
    [StswObservableProperty] string? _name;

    //TODO - remove
    /// <summary>
    /// Gets or sets the type of the database.
    /// </summary>
    [StswObservableProperty] StswDatabaseType _type = default;

    /// <summary>
    /// Gets or sets the server address of the database.
    /// </summary>
    [StswObservableProperty] string? _server;

    /// <summary>
    /// Gets or sets the port number of the database.
    /// </summary>
    [StswObservableProperty] int? _port;

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    [StswObservableProperty] string? _database;

    /// <summary>
    /// Gets or sets the login name for the database.
    /// </summary>
    [StswObservableProperty] string? _login;

    /// <summary>
    /// Gets or sets the password for the database.
    /// </summary>
    [StswObservableProperty] string? _password;

    /// <summary>
    /// Gets or sets the version of the database.
    /// </summary>
    [StswObservableProperty] string? _version;

    /// <summary>
    /// Gets or sets the default timeout for all commands associated with the database.
    /// </summary>
    [StswObservableProperty] int? _defaultTimeout;

    /// <summary>
    /// Gets or sets the encrypt mode for the connection.
    /// </summary>
    [StswObservableProperty] bool _encrypt;

    /// <summary>
    /// Constructs the connection string based on the model's properties.
    /// </summary>
    /// <returns>The database connection string.</returns>
    public string GetConnString()
    {
        if (string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(Database) || string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            throw new InvalidOperationException("Connection details are incomplete.");

        //TODO - change on ConnectionBuilder
        return Type switch
        {
            StswDatabaseType.MSSQL => $"Server={Server},{Port ?? 1433};Database={Database};User Id={Login};Password={Password};Encrypt={Encrypt};Application Name={StswFn.AppName()};",
            StswDatabaseType.MySQL => $"Server={Server};Port={Port ?? 3306};Database={Database};Uid={Login};Pwd={Password};Encrypt={Encrypt};Application Name={StswFn.AppName()};",
            StswDatabaseType.PostgreSQL => $"Server={Server};Port={Port ?? 5432};Database={Database};User Id={Login};Password={Password};Encrypt={Encrypt};Application Name={StswFn.AppName()};",
            _ => throw new Exception("This type of database management system is not supported!")
        };
    }

    /// <summary>
    /// Opens a new SQL connection using the connection string.
    /// </summary>
    /// <returns>An open <see cref="SqlConnection"/>.</returns>
    public SqlConnection OpenedConnection() => new SqlConnection(GetConnString()).GetOpened();
}
