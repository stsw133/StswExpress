using System;
using System.Data.SqlClient;

namespace StswExpress;
/// <summary>
/// Represents a model for database connection.
/// </summary>
public class StswDatabaseModel : StswObservableObject
{
    public StswDatabaseModel()
    {

    }
    public StswDatabaseModel(string? server = null, string? database = null, string? login = null, string? password = null)
    {
        Server = server;
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
    private StswDatabaseType _type = default;

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
    /// Constructs the connection string based on the model's properties.
    /// </summary>
    /// <returns>The database connection string.</returns>
    public string GetConnString() => Type switch
    {
        StswDatabaseType.MSSQL => $"Server={Server},{Port ?? 1433};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
        StswDatabaseType.MySQL => $"Server={Server};Port={Port ?? 3306};Database={Database};Uid={Login};Pwd={Password};Application Name={StswFn.AppName()};",
        StswDatabaseType.PostgreSQL => $"Server={Server};Port={Port ?? 5432};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
        _ => throw new Exception("This type of database management system is not supported!")
    };

    /// <summary>
    /// Opens a new SQL connection using the connection string.
    /// </summary>
    /// <returns>An open <see cref="SqlConnection"/>.</returns>
    public SqlConnection OpenedConnection()
    {
        var sqlConn = new SqlConnection(GetConnString());
        sqlConn.Open();
        return sqlConn;
    }

    /// <summary>
    /// Keeps the <see cref="System.Data.SqlClient.SqlTransaction"/> that has begun.
    /// </summary>
    internal SqlTransaction? SqlTransaction;

    /// <summary>
    /// Begins a new SQL transaction.
    /// </summary>
    public void BeginTransaction()
    {
        if (SqlTransaction != null)
            throw new Exception("SqlTransaction has been already started.");

        SqlTransaction = OpenedConnection().BeginTransaction();
    }

    /// <summary>
    /// Commits the current SQL transaction.
    /// </summary>
    public void CommitTransaction()
    {
        if (SqlTransaction == null)
            throw new Exception("SqlTransaction has already ended.");

        using (SqlTransaction)
            SqlTransaction.Commit();
    }

    /// <summary>
    /// Rolls back the current SQL transaction.
    /// </summary>
    public void RollbackTransaction()
    {
        if (SqlTransaction == null)
            throw new Exception("SqlTransaction has already ended.");

        using (SqlTransaction)
            SqlTransaction.Rollback();
    }

    /// <summary>
    /// Creates a new <see cref="StswQuery"/> using the current database connection.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <returns>A new <see cref="StswQuery"/> instance.</returns>
    public StswQuery Query(string query) => new(query, this);
}
