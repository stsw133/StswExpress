using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

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

    #region Main properties
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
    /// Gets or sets the default timeout for all commands associated with the database.
    /// </summary>
    public int? DefaultTimeout
    {
        get => _defaultTimeout;
        set => SetProperty(ref _defaultTimeout, value);
    }
    private int? _defaultTimeout;

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
    #endregion

    #region Query methods
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
    /// Begins a new SQL transaction.
    /// </summary>
    public void BeginTransaction()
    {
        if (_sqlTransaction != null)
            throw new Exception("SqlTransaction has been already started.");

        _sqlTransaction = OpenedConnection().BeginTransaction();
    }

    /// <summary>
    /// Commits the current SQL transaction.
    /// </summary>
    public void CommitTransaction()
    {
        if (_sqlTransaction == null)
            throw new Exception("SqlTransaction has already ended.");

        using (_sqlTransaction)
            _sqlTransaction.Commit();
    }

    /// <summary>
    /// Rolls back the current SQL transaction.
    /// </summary>
    public void RollbackTransaction()
    {
        if (_sqlTransaction == null)
            throw new Exception("SqlTransaction has already ended.");

        using (_sqlTransaction)
            _sqlTransaction.Rollback();
    }

    /// <summary>
    /// Gets or sets a value indicating whether to make less space in the query.
    /// </summary>
    public bool MakeLessSpaceQuery { get; set; } = StswDatabases.AlwaysMakeLessSpaceQuery;

    /// <summary>
    /// Gets or sets a value indicating whether to return if in designer mode.
    /// </summary>
    public bool ReturnIfInDesignerMode { get; set; } = StswDatabases.AlwaysReturnIfInDesignerMode;

    /// <summary>
    /// Gets or sets a value indicating whether to return if no database is available.
    /// </summary>
    public bool ReturnIfNoDatabase { get; set; } = StswDatabases.AlwaysReturnIfNoDatabase;

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public int? ExecuteNonQuery(string query, object? parameters = null, int? timeout = null)
    {
        if (!PrepareConnection())
            return default;

        IEnumerable<object?> models;
        if (parameters is IEnumerable<SqlParameter>)
            models = [parameters];
        else if (parameters is IEnumerable<object?> enumerable)
            models = enumerable;
        else
            models = [parameters];

        var result = 0;
        var sqlTran = _sqlTransaction ?? (models?.Count() > 1 ? _sqlConnection?.BeginTransaction() : null);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), _sqlConnection, sqlTran);
        sqlCmd.CommandTimeout = timeout ?? DefaultTimeout ?? sqlCmd.CommandTimeout;
        foreach (var model in models!)
        {
            PrepareParameters(sqlCmd, model);
            result += sqlCmd.ExecuteNonQuery();
        }

        /// commit only if one-time transaction
        if (_sqlTransaction == null)
            sqlTran?.Commit();

        return result;
    }

    /// <summary>
    /// Executes the query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The scalar value.</returns>
    public TResult ExecuteScalar<TResult>(string query, object? parameters = null, int? timeout = null)
    {
        if (!PrepareConnection())
            return default!;

        using var sqlCmd = new SqlCommand(PrepareQuery(query), _sqlConnection, _sqlTransaction);
        sqlCmd.CommandTimeout = timeout ?? DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteScalar().ConvertTo<TResult>()!;
    }

    /// <summary>
    /// Executes the query and returns a scalar value or default if the query fails.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The scalar value or default.</returns>
    public TResult? TryExecuteScalar<TResult>(string query, object? parameters = null, int? timeout = null)
    {
        if (!PrepareConnection())
            return default;

        using var sqlCmd = new SqlCommand(PrepareQuery(query), _sqlConnection, _sqlTransaction);
        sqlCmd.CommandTimeout = timeout ?? DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        using var sqlDR = sqlCmd.ExecuteReader();
        return sqlDR.Read() ? sqlDR[0].ConvertTo<TResult>() : default;
    }

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    public SqlDataReader? ExecuteReader(string query, object? parameters = null, int? timeout = null)
    {
        if (!PrepareConnection())
            return default;

        var sqlCmd = new SqlCommand(PrepareQuery(query), _sqlConnection, _sqlTransaction);
        sqlCmd.CommandTimeout = timeout ?? DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A collection of results.</returns>
    public IEnumerable<TResult> Get<TResult>(string query, object? parameters = null, int? timeout = null) where TResult : class, new()
    {
        if (!PrepareConnection())
            return default!;

        using var sqlDA = new SqlDataAdapter(PrepareQuery(query), _sqlConnection);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? DefaultTimeout ?? sqlDA.SelectCommand.CommandTimeout;
        PrepareParameters(sqlDA.SelectCommand, parameters);

        var dt = new DataTable();
        sqlDA.Fill(dt);
        return dt.MapTo<TResult>('/');
    }

    /// <summary>
    /// Executes the query and updates the database with the specified input collection.
    /// </summary>
    /// <typeparam name="TModel">The type of the items in the input collection.</typeparam>
    /// <param name="input">The input collection.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="idProp">The property name of the ID.</param>
    /// <param name="inclusionMode">The inclusion mode.</param>
    /// <param name="inclusionProps">The properties to include or exclude based on the inclusion mode.</param>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    [Obsolete($"This method should be replaced with multiple {nameof(ExecuteNonQuery)}")]
    public void Set<TModel>(StswBindingList<TModel> input, string tableName, string idProp, StswInclusionMode inclusionMode = StswInclusionMode.Include, IEnumerable<string>? inclusionProps = null, IList<SqlParameter>? sqlParameters = null) where TModel : IStswCollectionItem, new()
    {
        if (!PrepareConnection())
            return;

        /// prepare parameters
        inclusionProps ??= [];
        sqlParameters ??= [];

        var objProps = inclusionMode == StswInclusionMode.Include
            ? typeof(TModel).GetProperties().Where(x => x.Name.In(inclusionProps))
            : typeof(TModel).GetProperties().Where(x => !x.Name.In(inclusionProps.Union(input.IgnoredProperties)));
        var objPropsWithoutID = objProps.Where(x => x.Name != idProp);

        /// func
        using var sqlTran = _sqlTransaction ?? _sqlConnection?.BeginTransaction();

        var insertQuery = $"insert into {tableName} ({string.Join(", ", objPropsWithoutID.Select(x => x.Name))}) values ({string.Join(", ", objPropsWithoutID.Select(x => "@" + x.Name))})";
        foreach (var item in input.GetItemsByState(StswItemState.Added))
            using (var sqlCmd = new SqlCommand(insertQuery, _sqlConnection, sqlTran))
            {
                sqlCmd.Parameters.AddRange([.. sqlParameters]);
                PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                sqlCmd.ExecuteNonQuery();
            }

        var updateQuery = $"update {tableName} set {string.Join(", ", objPropsWithoutID.Select(x => $"{x.Name}=@{x.Name}"))} where {idProp}=@{idProp}";
        foreach (var item in input.GetItemsByState(StswItemState.Modified))
            using (var sqlCmd = new SqlCommand(updateQuery, _sqlConnection, sqlTran))
            {
                sqlCmd.Parameters.AddRange([.. sqlParameters]);
                PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                sqlCmd.ExecuteNonQuery();
            }

        var deleteQuery = $"delete from {tableName} where {idProp}=@{idProp}";
        foreach (var item in input.GetItemsByState(StswItemState.Deleted))
            using (var sqlCmd = new SqlCommand(deleteQuery, _sqlConnection, sqlTran))
            {
                sqlCmd.Parameters.AddWithValue("@" + idProp, objProps.First(x => x.Name == idProp).GetValue(item));
                sqlCmd.ExecuteNonQuery();
            }

        /// commit only if one-time transaction
        if (_sqlTransaction == null)
            sqlTran?.Commit();
    }

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    public void BulkInsert<TModel>(IEnumerable<TModel> items, string tableName, int? timeout = null)
    {
        if (!PrepareConnection())
            return;

        using var bulkCopy = new SqlBulkCopy(_sqlConnection, SqlBulkCopyOptions.Default, _sqlTransaction);
        bulkCopy.BulkCopyTimeout = timeout ?? DefaultTimeout ?? bulkCopy.BulkCopyTimeout;
        bulkCopy.DestinationTableName = tableName;
        var dataTable = items.ToDataTable();
        bulkCopy.WriteToServer(dataTable);
    }

    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="procName">The name of the stored procedure.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public int? ExecuteStoredProcedure(string procName, object? parameters = null, int? timeout = null)
    {
        if (!PrepareConnection())
            return default;

        using var sqlCmd = new SqlCommand(procName, _sqlConnection, _sqlTransaction) { CommandType = CommandType.StoredProcedure };
        sqlCmd.CommandTimeout = timeout ?? DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteNonQuery();
    }
    #endregion

    #region Query helpers
    /// <summary>
    /// Reduces the amount of space in the query by removing unnecessary whitespace.
    /// </summary>
    /// <param name="query">The SQL query to process.</param>
    /// <returns>The processed SQL query.</returns>
    public static string LessSpaceQuery(string query)
    {
        var regex = new Regex(@"('([^']*)')|([^']+)");
        var parts = regex.Matches(query)
                         .Cast<Match>()
                         .Select(match => match.Groups[2].Success ? (match.Groups[2].Value, true) : (match.Groups[3].Value, false))
                         .ToList();

        return parts.Aggregate(string.Empty, (current, part) => current + (part.Item2 ? $"'{part.Value}'" : StswFn.RemoveConsecutiveText(part.Value.Replace("\t", " "), " ")));
    }

    /// <summary>
    /// Prepares the SQL connection and transaction.
    /// </summary>
    /// <returns>true if the connection is successfully prepared; otherwise, false.</returns>
    protected bool PrepareConnection()
    {
        var isInDesignMode = false;
        if (ReturnIfInDesignerMode)
            Application.Current.Dispatcher.Invoke(() => isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject()));
        if (isInDesignMode)
            return false;

        if (_sqlTransaction != null)
            _sqlConnection = _sqlTransaction.Connection;
        else
            _sqlConnection = OpenedConnection();

        if (_sqlConnection == null)
        {
            if (ReturnIfNoDatabase)
                return false;
            throw new InvalidOperationException("Connection could not be prepared.");
        }
        if (_sqlConnection?.State != ConnectionState.Open)
            _sqlConnection?.Open();

        return true;
    }
    private SqlConnection? _sqlConnection;
    private SqlTransaction? _sqlTransaction;

    /// <summary>
    /// Prepares the SQL command with the specified parameters.
    /// </summary>
    /// <param name="sqlCommand">The SQL command to prepare.</param>
    /// <param name="model">The model used for the query parameters.</param>
    public static void PrepareParameters(SqlCommand sqlCommand, object? model)
    {
        sqlCommand.Parameters.Clear();
        if (model != null)
        {
            var sqlParameters = model is IEnumerable<SqlParameter> parameters
                ? parameters.ToList()
                : model.GetType().GetProperties().Select(prop => new SqlParameter("@" + prop.Name, prop.GetValue(model) ?? DBNull.Value)).ToList();

            foreach (var parameter in sqlParameters)
            {
                if (parameter.Value.GetType().IsListType(out var type) && type?.IsValueType == true)
                    sqlCommand.ParametersAddList(parameter.ParameterName, (IList?)parameter.Value);
                else
                    sqlCommand.Parameters.Add(parameter);
            }
        }
    }

    /// <summary>
    /// Prepares the SQL command with the specified parameters and properties.
    /// </summary>
    /// <typeparam name="TModel">The type of the item containing the properties.</typeparam>
    /// <param name="sqlCommand">The SQL command to prepare.</param>
    /// <param name="sqlParameters">The SQL parameters to add to the command.</param>
    /// <param name="propertyInfos">The properties to add as parameters.</param>
    /// <param name="item">The item containing the properties.</param>
    protected void PrepareParameters<TModel>(SqlCommand sqlCommand, IEnumerable<SqlParameter>? sqlParameters, IEnumerable<PropertyInfo>? propertyInfos, TModel? item)
    {
        /// add parameters
        PrepareParameters(sqlCommand, sqlParameters);

        /// add properties as parameters
        if (propertyInfos != null && item != null)
        {
            foreach (var prop in propertyInfos)
            {
                if (!sqlCommand.Parameters.Contains("@" + prop.Name))
                {
                    var value = prop.GetValue(item);
                    if (value == null)
                    {
                        sqlCommand.Parameters.Add("@" + prop.Name, prop.PropertyType.InferSqlDbType()!.Value).Value = DBNull.Value;
                    }
                    else if (prop.PropertyType.IsListType(out var type) && type?.IsValueType == true)
                    {
                        if (type == typeof(byte))
                            sqlCommand.Parameters.AddWithValue("@" + prop.Name, (byte[])value);
                        else
                            sqlCommand.ParametersAddList("@" + prop.Name, (IList?)value);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@" + prop.Name, value ?? DBNull.Value);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Prepares the SQL query for execution.
    /// </summary>
    /// <param name="query">The SQL query to prepare.</param>
    /// <returns>The prepared SQL query.</returns>
    protected string PrepareQuery(string query) => MakeLessSpaceQuery ? LessSpaceQuery(query) : query;
    #endregion

    ~StswDatabaseModel()
    {
        _sqlTransaction?.Rollback();
        _sqlTransaction?.Dispose();
        _sqlTransaction = null;

        _sqlConnection?.Close();
        _sqlConnection?.Dispose();
        _sqlConnection = null;
    }
}
