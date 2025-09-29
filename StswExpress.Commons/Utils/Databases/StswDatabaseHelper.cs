using Microsoft.Data.SqlClient;
using System.Collections;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StswExpress.Commons;

/// <summary>
/// A static helper class that provides extension methods for performing common database operations 
/// such as bulk inserts, executing queries, and handling stored procedures in a more efficient manner.
/// </summary>
[StswInfo("0.9.2")]
public static partial class StswDatabaseHelper
{
    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex BlockCommentsRegex();

    [GeneratedRegex(@"('([^']*)')|([^']+)")]
    private static partial Regex LessSpaceRegex();

    [GeneratedRegex(@"--.*?$", RegexOptions.Multiline)]
    private static partial Regex LineCommentsRegex();

    [GeneratedRegex(@"@(\w+)")]
    private static partial Regex ParameterRegex();

    [GeneratedRegex(@"'[^']*'")]
    private static partial Regex SingleQuotesRegex();

    /// <summary>
    /// Opens a new SQL connection using the connection string.
    /// </summary>
    /// <returns>An open <see cref="SqlConnection"/>.</returns>
    public static SqlConnection GetOpened(this SqlConnection connection)
    {
        if (connection.State != ConnectionState.Open)
            connection.Open();
        return connection;
    }

    /// <summary>
    /// Opens a new SQL connection using the connection string from the provided <see cref="StswDatabaseModel"/>.
    /// </summary>
    /// <returns>An open <see cref="SqlConnection"/>.</returns>
    [StswInfo("0.9.2", "0.20.0")]
    public static SqlConnection OpenedConnection(this StswDatabaseModel model) => new SqlConnection(model.GetConnString()).GetOpened();

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    [StswInfo("0.9.3", "0.19.0")]
    public static void BulkInsert<TModel>(this SqlConnection sqlConn, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (typeof(IDictionary<string, object?>).IsAssignableFrom(typeof(TModel)))
        {
            sqlConn.BulkInsert((IEnumerable)items, tableName, timeout, sqlTran, disposeConnection);
            return;
        }

        if (!CheckQueryConditions())
            return;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, disposeConnection);

        using var sqlBulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        sqlBulkCopy.BulkCopyTimeout = timeout ?? sqlBulkCopy.BulkCopyTimeout;
        sqlBulkCopy.DestinationTableName = tableName;

        var dt = items.ToDataTable();
        foreach (DataColumn column in dt.Columns)
            sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        // add indexes
        sqlBulkCopy.WriteToServer(dt);

        factory.Commit();
    }

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.9.3", "0.19.0")]
    public static void BulkInsert<TModel>(this SqlTransaction sqlTran, IEnumerable<TModel> items, string tableName, int? timeout = null)
        => sqlTran.Connection.BulkInsert(items, tableName, timeout, sqlTran);

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.9.2", "0.19.0")]
    public static void BulkInsert<TModel>(this StswDatabaseModel model, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().BulkInsert(items, tableName, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    [StswInfo("0.21.0")]
    public static void BulkInsert(this SqlConnection sqlConn, IEnumerable items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, disposeConnection);

        using var sqlBulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        sqlBulkCopy.BulkCopyTimeout = timeout ?? sqlBulkCopy.BulkCopyTimeout;
        sqlBulkCopy.DestinationTableName = tableName;

        var dt = items.ToDataTable();
        if (dt.Columns.Count == 0)
            throw new InvalidOperationException("Cannot infer column definitions from the provided items.");
        foreach (DataColumn column in dt.Columns)
            sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        // add indexes
        sqlBulkCopy.WriteToServer(dt);

        factory.Commit();
    }

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.21.0")]
    public static void BulkInsert(this SqlTransaction sqlTran, IEnumerable items, string tableName, int? timeout = null)
        => sqlTran.Connection.BulkInsert(items, tableName, timeout, sqlTran);

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.21.0")]
    public static void BulkInsert(this StswDatabaseModel model, IEnumerable items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().BulkInsert(items, tableName, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Executes a non-query SQL command and returns the number of rows affected.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.3")]
    public static int? ExecuteNonQuery(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return default;

        var parameterModels = parameters switch
        {
            IEnumerable<SqlParameter> => [parameters],
            IEnumerable<object?> enumerable => enumerable,
            _ => [parameters],
        };

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, parameterModels.Count() > 1, disposeConnection);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;

        var result = 0;

        foreach (var parameterModel in parameterModels)
            result += sqlCmd.PrepareCommand(parameterModel).ExecuteNonQuery();

        factory.Commit();
        return result;
    }

    /// <summary>
    /// Executes a non-query SQL command and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.3")]
    public static int? ExecuteNonQuery(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteNonQuery(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a non-query SQL command and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.2")]
    public static int? ExecuteNonQuery(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteNonQuery(query, parameters, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A <see cref="SqlDataReader"/> instance for reading the data, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.3")]
    public static SqlDataReader ExecuteReader(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
    {
        if (!CheckQueryConditions())
            return default!;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false, false);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        sqlCmd.PrepareCommand(parameters);

        return factory.Transaction != null ? sqlCmd.ExecuteReader() : sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// Executes a SQL query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A <see cref="SqlDataReader"/> instance for reading the data, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.3")]
    public static SqlDataReader ExecuteReader(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteReader(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A <see cref="SqlDataReader"/> instance for reading the data, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.2")]
    public static SqlDataReader ExecuteReader(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteReader(query, parameters, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>The scalar value, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.3")]
    public static TResult ExecuteScalar<TResult>(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return default!;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false, disposeConnection);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        var result = sqlCmd.PrepareCommand(parameters).ExecuteScalar().ConvertTo<TResult?>();

        factory.Commit();
        return result!;
    }

    /// <summary>
    /// Executes a SQL query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>The scalar value, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.3")]
    public static TResult ExecuteScalar<TResult>(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteScalar<TResult>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>The scalar value, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.2")]
    public static TResult ExecuteScalar<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteScalar<TResult>(query, parameters, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Executes a stored procedure with parameters and returns the number of rows affected.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="procName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">The model used for the stored procedure parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.3")]
    public static int? ExecuteStoredProcedure(this SqlConnection sqlConn, string procName, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return default;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, disposeConnection);
        using var sqlCmd = new SqlCommand(procName, factory.Connection, factory.Transaction) { CommandType = CommandType.StoredProcedure, };
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        var result = sqlCmd.PrepareCommand(parameters, passAllParametersAnyway: true).ExecuteNonQuery();

        factory.Commit();
        return result;
    }

    /// <summary>
    /// Executes a stored procedure with parameters and returns the number of rows affected.
    /// </summary>
    /// <param name="procName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">The model used for the stored procedure parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.3")]
    public static int? ExecuteStoredProcedure(this SqlTransaction sqlTran, string procName, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteStoredProcedure(procName, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a stored procedure with parameters and returns the number of rows affected.
    /// </summary>
    /// <param name="procName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">The model used for the stored procedure parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    [StswInfo("0.9.2")]
    public static int? ExecuteStoredProcedure(this StswDatabaseModel model, string procName, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteStoredProcedure(procName, parameters, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results to return.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    [StswInfo("0.9.3", "0.20.0")]
    public static IEnumerable<TResult> Get<TResult>(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return [];

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false, disposeConnection);
        using var sqlDA = new SqlDataAdapter(PrepareQuery(query), factory.Connection);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? sqlDA.SelectCommand.CommandTimeout;
        sqlDA.SelectCommand.Transaction = factory.Transaction;
        sqlDA.SelectCommand.PrepareCommand(parameters);

        var dt = new DataTable();
        sqlDA.Fill(dt);

        var delimiter = StswDatabases.Config.DelimiterForMapping;
        var hasDelimiter = dt.Columns.Cast<DataColumn>()
            .Any(col => col.ColumnName.Contains(delimiter));

        return hasDelimiter
            ? dt.MapTo<TResult>(delimiter)
            : dt.MapTo<TResult>();
    }

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    [StswInfo("0.9.3", "0.20.0")]
    public static IEnumerable<TResult> Get<TResult>(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.Get<TResult>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    [StswInfo("0.9.2", "0.20.0")]
    public static IEnumerable<TResult> Get<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().Get<TResult>(query, parameters, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="type">The type of the results to return.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    [StswInfo("0.18.0", "0.19.0")]
    public static IEnumerable<object?> Get(this SqlConnection sqlConn, Type type, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return [];

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false, disposeConnection);
        using var sqlDA = new SqlDataAdapter(PrepareQuery(query), factory.Connection);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? sqlDA.SelectCommand.CommandTimeout;
        sqlDA.SelectCommand.Transaction = factory.Transaction;
        sqlDA.SelectCommand.PrepareCommand(parameters);

        var dt = new DataTable();
        sqlDA.Fill(dt);

        var delimiter = StswDatabases.Config.DelimiterForMapping;
        var hasDelimiter = dt.Columns.Cast<DataColumn>()
            .Any(col => col.ColumnName.Contains(delimiter));

        return hasDelimiter
            ? dt.MapTo(type, delimiter)
            : dt.MapTo(type);
    }

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <param name="type">The type of the results to return.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    [StswInfo("0.18.0", "0.19.0")]
    public static IEnumerable<object?> Get(this SqlTransaction sqlTran, Type type, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.Get(type, query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <param name="type">The type of the results to return.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    [StswInfo("0.18.0", "0.19.0")]
    public static IEnumerable<object?> Get(this StswDatabaseModel model, Type type, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().Get(type, query, parameters, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query that returns combined data for both header and item entities, separates the result into two model types,
    /// and injects the corresponding items into each header based on a shared key.
    /// </summary>
    /// <typeparam name="THeader">The type representing the header part of the result.</typeparam>
    /// <typeparam name="TItem">The type representing the item (detail) part of the result.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query that returns both header and item columns in one result set.</param>
    /// <param name="joinKeys">The property names present in both <typeparamref name="THeader"/> and <typeparamref name="TItem"/> used to join items with headers.</param>
    /// <param name="injectIntoProperty">The name of the collection property in <typeparamref name="THeader"/> where the related <typeparamref name="TItem"/> objects should be assigned.</param>
    /// <param name="divideFromColumn">The name of the first column in the result set that belongs to the item model. This column and all following columns are considered item data.</param>
    /// <param name="parameters">Optional. The parameters used for the SQL query, if any.</param>
    /// <param name="timeout">Optional. The command timeout in seconds. If <see langword="null"/>, the default is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to associate with the query. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>A list of <typeparamref name="THeader"/> objects, each with an associated collection of <typeparamref name="TItem"/> objects injected into the specified property.</returns>
    [StswInfo("0.13.0", PlannedChanges = StswPlannedChanges.Remove)]
    public static IEnumerable<THeader> GetDivided<THeader, TItem>(this SqlConnection sqlConn, string query, KeyValuePair<string, string?> joinKeys, string injectIntoProperty, string divideFromColumn, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (string.IsNullOrWhiteSpace(joinKeys.Key)
         || string.IsNullOrWhiteSpace(injectIntoProperty)
         || string.IsNullOrWhiteSpace(divideFromColumn))
            throw new ArgumentException("One or more required arguments are null or empty.");

        var headerKeyProp = typeof(THeader).GetProperty(joinKeys.Key) ?? throw new ArgumentException($"Property '{joinKeys.Key}' not found in {typeof(THeader).Name}.");
        var injectProp = typeof(THeader).GetProperty(injectIntoProperty) ?? throw new ArgumentException($"Property '{injectIntoProperty}' not found in {typeof(THeader).Name}.");
        if (!injectProp.PropertyType.IsListType(out var itemType))
            throw new ArgumentException($"{injectIntoProperty} must be a collection type.");

        PropertyInfo? itemKeyProp = null;
        if (!string.IsNullOrWhiteSpace(joinKeys.Value))
            itemKeyProp = typeof(TItem).GetProperty(joinKeys.Value!)
                ?? throw new ArgumentException($"Property '{joinKeys.Value}' not found in {typeof(TItem).Name}.");

        if (!CheckQueryConditions())
            return [];

        var getHeaderKey = BuildGetter(headerKeyProp);
        var getItemKey = itemKeyProp is not null ? BuildGetter(itemKeyProp) : null;

        var listCtor = Expression.Lambda<Func<IList>>(Expression.Convert(Expression.New(typeof(List<>).MakeGenericType(typeof(TItem))), typeof(IList))).Compile();

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false, disposeConnection);
        using var sqlDA = new SqlDataAdapter(PrepareQuery(query), factory.Connection);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? sqlDA.SelectCommand.CommandTimeout;
        sqlDA.SelectCommand.Transaction = factory.Transaction;
        sqlDA.SelectCommand.PrepareCommand(parameters);

        var fullTable = new DataTable();
        sqlDA.Fill(fullTable);

        var dividerIndex = fullTable.Columns.Cast<DataColumn>()
            .Select((col, idx) => new { col, idx })
            .LastOrDefault(c =>
                TrimTrailingDigits(c.col.ColumnName).Equals(divideFromColumn, StringComparison.OrdinalIgnoreCase)
            )?.idx ?? -1;

        if (dividerIndex < 0)
            throw new ArgumentException($"Column matching '{divideFromColumn}' not found in result set.");

        var headerTable = new DataTable();
        var itemTable = new DataTable();

        var columnRenameMap = GetColumnRenameMap(fullTable, typeof(TItem));

        for (var i = 0; i < dividerIndex; i++)
        {
            var col = fullTable.Columns[i];
            headerTable.Columns.Add(col.ColumnName, col.DataType);
        }

        for (var i = dividerIndex; i < fullTable.Columns.Count; i++)
        {
            var originalName = fullTable.Columns[i].ColumnName;
            var targetName = columnRenameMap.TryGetValue(originalName, out var newName) ? newName : originalName;
            itemTable.Columns.Add(targetName, fullTable.Columns[i].DataType);
        }

        var rowKeyColumn = fullTable.Columns[joinKeys.Key];
        var rowKeyToItems = new Dictionary<object, List<TItem>>();

        foreach (DataRow row in fullTable.Rows)
        {
            var headerRow = headerTable.NewRow();
            var itemRow = itemTable.NewRow();

            for (var i = 0; i < dividerIndex; i++)
                headerRow[i] = row[i];

            for (var i = dividerIndex; i < fullTable.Columns.Count; i++)
            {
                var originalName = fullTable.Columns[i].ColumnName;
                var targetName = columnRenameMap.TryGetValue(originalName, out var newName) ? newName : originalName;
                itemRow[targetName] = row[i];
            }

            headerTable.Rows.Add(headerRow);
            itemTable.Rows.Add(itemRow);
        }

        var headers = headerTable
            .MapTo<THeader>(StswDatabases.Config.DelimiterForMapping)
            .GroupBy(x => getHeaderKey(x!)!)
            .Select(g => g.First())
            .ToList();
        var items = itemTable
            .MapTo<TItem>(StswDatabases.Config.DelimiterForMapping)
            .ToList();

        if (itemKeyProp is not null)
        {
            var itemLookup = items
                .Where(x => getItemKey!(x!) is not null)
                .ToLookup(x => getItemKey!(x!)!);

            foreach (var header in headers)
            {
                var headerKey = getHeaderKey(header!);
                var targetList = listCtor();

                foreach (var item in itemLookup[headerKey!])
                    targetList.Add(item);

                injectProp.SetValue(header, targetList);
            }
        }
        else
        {
            for (var i = 0; i < fullTable.Rows.Count; i++)
            {
                var rowKey = fullTable.Rows[i][rowKeyColumn!];
                if (!rowKeyToItems.TryGetValue(rowKey, out var list))
                    rowKeyToItems[rowKey] = list = [];

                list.Add(items[i]!);
            }

            foreach (var header in headers)
            {
                var key = getHeaderKey(header!);
                rowKeyToItems.TryGetValue(key!, out var list);
                injectProp.SetValue(header, list ?? listCtor());
            }
        }

        return headers!;
    }

    /// <summary>
    /// Executes a SQL query that returns combined data for both header and item entities, separates the result into two model types,
    /// and injects the corresponding items into each header based on a shared key.
    /// </summary>
    /// <typeparam name="THeader">The type representing the header part of the result.</typeparam>
    /// <typeparam name="TItem">The type representing the item (detail) part of the result.</typeparam>
    /// <param name="query">The SQL query that returns both header and item columns in one result set.</param>
    /// <param name="joinKeys">The property names present in both <typeparamref name="THeader"/> and <typeparamref name="TItem"/> used to join items with headers.</param>
    /// <param name="injectIntoProperty">The name of the collection property in <typeparamref name="THeader"/> where the related <typeparamref name="TItem"/> objects should be assigned.</param>
    /// <param name="divideFromColumn">The name of the first column in the result set that belongs to the item model. This column and all following columns are considered item data.</param>
    /// <param name="parameters">Optional. The parameters used for the SQL query, if any.</param>
    /// <param name="timeout">Optional. The command timeout in seconds. If <see langword="null"/>, the default is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to associate with the query. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A list of <typeparamref name="THeader"/> objects, each with an associated collection of <typeparamref name="TItem"/> objects injected into the specified property.</returns>
    [StswInfo("0.13.0", PlannedChanges = StswPlannedChanges.Remove)]
    public static IEnumerable<THeader> GetDivided<THeader, TItem>(this SqlTransaction sqlTran, string query, KeyValuePair<string, string?> joinKeys, string injectIntoProperty, string divideFromColumn, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.GetDivided<THeader, TItem>(query, joinKeys, injectIntoProperty, divideFromColumn, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query that returns combined data for both header and item entities, separates the result into two model types,
    /// and injects the corresponding items into each header based on a shared key.
    /// </summary>
    /// <typeparam name="THeader">The type representing the header part of the result.</typeparam>
    /// <typeparam name="TItem">The type representing the item (detail) part of the result.</typeparam>
    /// <param name="query">The SQL query that returns both header and item columns in one result set.</param>
    /// <param name="joinKeys">The property names present in both <typeparamref name="THeader"/> and <typeparamref name="TItem"/> used to join items with headers.</param>
    /// <param name="injectIntoProperty">The name of the collection property in <typeparamref name="THeader"/> where the related <typeparamref name="TItem"/> objects should be assigned.</param>
    /// <param name="divideFromColumn">The name of the first column in the result set that belongs to the item model. This column and all following columns are considered item data.</param>
    /// <param name="parameters">Optional. The parameters used for the SQL query, if any.</param>
    /// <param name="timeout">Optional. The command timeout in seconds. If <see langword="null"/>, the default is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to associate with the query. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A list of <typeparamref name="THeader"/> objects, each with an associated collection of <typeparamref name="TItem"/> objects injected into the specified property.</returns>
    [StswInfo("0.13.0", PlannedChanges = StswPlannedChanges.Remove)]
    public static IEnumerable<THeader> GetDivided<THeader, TItem>(this StswDatabaseModel model, string query, KeyValuePair<string, string?> joinKeys, string injectIntoProperty, string divideFromColumn, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().GetDivided<THeader, TItem>(query, joinKeys, injectIntoProperty, divideFromColumn, parameters, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Inserts a collection of items into a temporary SQL table. The method dynamically creates the temporary table
    /// based on the structure of the data model and uses <see cref="SqlBulkCopy"/> to efficiently insert the data.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the temporary table to be created and populated.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.9.3", "0.20.0")]
    public static void TempTableInsert<TModel>(this SqlConnection sqlConn, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
    {
        if (typeof(IDictionary<string, object?>).IsAssignableFrom(typeof(TModel)))
        {
            sqlConn.TempTableInsert((IEnumerable)items, tableName, timeout, sqlTran);
            return;
        }

        if (!CheckQueryConditions())
            return;

        var dt = items.ToDataTable();
        var columnsDefinitions = dt.Columns
            .Cast<DataColumn>()
            .Select(col => $"[{col.ColumnName}] {GetSqlType(col.DataType)}");
        tableName = "#" + tableName.TrimStart('#');

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, false);
        using var sqlCmd = new SqlCommand($"CREATE TABLE {tableName} ({string.Join(", ", columnsDefinitions)});", factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        sqlCmd.ExecuteNonQuery();
        
        using var sqlBulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        sqlBulkCopy.BulkCopyTimeout = timeout ?? sqlBulkCopy.BulkCopyTimeout;
        sqlBulkCopy.DestinationTableName = tableName;

        foreach (DataColumn column in dt.Columns)
            sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        // add indexes
        sqlBulkCopy.WriteToServer(dt);

        factory.Commit();
    }

    /// <summary>
    /// Inserts a collection of items into a temporary SQL table. The method dynamically creates the temporary table
    /// based on the structure of the data model and uses <see cref="SqlBulkCopy"/> to efficiently insert the data.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the temporary table to be created and populated.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.9.3", "0.20.0")]
    public static void TempTableInsert<TModel>(this SqlTransaction sqlTran, IEnumerable<TModel> items, string tableName, int? timeout = null)
        => sqlTran.Connection.TempTableInsert(items, tableName, timeout, sqlTran);

    /// <summary>
    /// Inserts a collection of items into a temporary SQL table. The method dynamically creates the temporary table
    /// based on the structure of the data model and uses <see cref="SqlBulkCopy"/> to efficiently insert the data.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the temporary table to be created and populated.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.9.2", "0.20.0")]
    public static void TempTableInsert<TModel>(this StswDatabaseModel model, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().TempTableInsert(items, tableName, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Inserts a collection of items into a temporary SQL table. The method dynamically creates the temporary table
    /// based on the structure of the data model and uses <see cref="SqlBulkCopy"/> to efficiently insert the data.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the temporary table to be created and populated.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.21.0")]
    public static void TempTableInsert(this SqlConnection sqlConn, IEnumerable items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
    {
        if (!CheckQueryConditions())
            return;

        var dt = items.ToDataTable();
        if (dt.Columns.Count == 0)
            throw new InvalidOperationException("Cannot infer column definitions from the provided items.");

        var columnsDefinitions = dt.Columns
            .Cast<DataColumn>()
            .Select(col => $"[{col.ColumnName}] {GetSqlType(col.DataType)}");
        tableName = "#" + tableName.TrimStart('#');

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, false);
        using var sqlCmd = new SqlCommand($"CREATE TABLE {tableName} ({string.Join(", ", columnsDefinitions)});", factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        sqlCmd.ExecuteNonQuery();

        using var sqlBulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        sqlBulkCopy.BulkCopyTimeout = timeout ?? sqlBulkCopy.BulkCopyTimeout;
        sqlBulkCopy.DestinationTableName = tableName;

        foreach (DataColumn column in dt.Columns)
            sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        // add indexes
        sqlBulkCopy.WriteToServer(dt);

        factory.Commit();
    }

    /// <summary>
    /// Inserts a collection of items into a temporary SQL table. The method dynamically creates the temporary table
    /// based on the structure of the data model and uses <see cref="SqlBulkCopy"/> to efficiently insert the data.
    /// </summary>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the temporary table to be created and populated.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.21.0")]
    public static void TempTableInsert(this SqlTransaction sqlTran, IEnumerable items, string tableName, int? timeout = null)
        => sqlTran.Connection.TempTableInsert(items, tableName, timeout, sqlTran);

    /// <summary>
    /// Inserts a collection of items into a temporary SQL table. The method dynamically creates the temporary table
    /// based on the structure of the data model and uses <see cref="SqlBulkCopy"/> to efficiently insert the data.
    /// </summary>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the temporary table to be created and populated.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    [StswInfo("0.21.0")]
    public static void TempTableInsert(this StswDatabaseModel model, IEnumerable items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().TempTableInsert(items, tableName, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Performs insert, update, and delete operations on a SQL table based on the state of the items in the provided <see cref="StswObservableCollection{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the items in the list.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="items">The list of items to insert, update, or delete.</param>
    /// <param name="tableName">The name of the SQL table to modify.</param>
    /// <param name="setColumns">The columns to be updated in the table.</param>
    /// <param name="idColumns">The columns used as identifiers in the table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <remarks>
    /// This method assumes that the column names in the SQL table match the property names in the <see cref="StswObservableCollection{TModel}"/>.
    /// </remarks>
    [StswInfo("0.9.3", PlannedChanges = StswPlannedChanges.Remove)]
    public static void Set<TModel>(this SqlConnection sqlConn, StswObservableCollection<TModel> items, string tableName, IEnumerable<string>? setColumns = null, IEnumerable<string>? idColumns = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null) where TModel : IStswCollectionItem, new()
    {
        if (!CheckQueryConditions())
            return;

        idColumns ??= ["ID"];
        setColumns ??= typeof(TModel).GetProperties().Select(x => x.Name);
        setColumns = setColumns.Except(items.IgnoredPropertyNames.Append(nameof(IStswCollectionItem.ItemState)));

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, disposeConnection);

        var insertQuery = $"insert into {tableName} ({string.Join(',', setColumns)}) values ({string.Join(',', setColumns.Select(x => "@" + x))})";
        using (var sqlCmd = new SqlCommand(PrepareQuery(insertQuery), factory.Connection, factory.Transaction))
        {
            sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
            foreach (var item in items.AddedItems)
                sqlCmd.PrepareCommand(GenerateSqlParameters(item, setColumns, idColumns, item.ItemState)).ExecuteNonQuery();
        }
        
        var updateQuery = $"update {tableName} set {string.Join(',', setColumns.Select(x => x + "=@" + x))} where {string.Join(',', idColumns.Select(x => x + "=@" + x))}";
        using (var sqlCmd = new SqlCommand(PrepareQuery(updateQuery), factory.Connection, factory.Transaction))
        {
            sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
            foreach (var item in items.ModifiedItems)
                sqlCmd.PrepareCommand(GenerateSqlParameters(item, setColumns, idColumns, item.ItemState)).ExecuteNonQuery();
        }
        
        var deleteQuery = $"delete from {tableName} where {string.Join(',', idColumns.Select(x => x + "=@" + x))}";
        using (var sqlCmd = new SqlCommand(PrepareQuery(deleteQuery), factory.Connection, factory.Transaction))
        {
            sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
            foreach (var item in items.DeletedItems)
                PrepareCommand(sqlCmd, GenerateSqlParameters(item, setColumns, idColumns, item.ItemState)).ExecuteNonQuery();
        }

        factory.Commit();
    }

    /// <summary>
    /// Performs insert, update, and delete operations on a SQL table based on the state of the items in the provided <see cref="StswObservableCollection{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the items in the list.</typeparam>
    /// <param name="items">The list of items to insert, update, or delete.</param>
    /// <param name="tableName">The name of the SQL table to modify.</param>
    /// <param name="setColumns">The columns to be updated in the table.</param>
    /// <param name="idColumns">The columns used as identifiers in the table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <remarks>
    /// This method assumes that the column names in the SQL table match the property names in the <see cref="StswObservableCollection{T}{TModel}"/>.
    /// </remarks>
    [StswInfo("0.9.3", PlannedChanges = StswPlannedChanges.Remove)]
    public static void Set<TModel>(this SqlTransaction sqlTran, StswObservableCollection<TModel> items, string tableName, IEnumerable<string>? setColumns = null, IEnumerable<string>? idColumns = null, int? timeout = null) where TModel : IStswCollectionItem, new()
        => sqlTran.Connection.Set(items, tableName, setColumns, idColumns, timeout, sqlTran);

    /// <summary>
    /// Performs insert, update, and delete operations on a SQL table based on the state of the items in the provided <see cref="StswObservableCollection{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the items in the list.</typeparam>
    /// <param name="items">The list of items to insert, update, or delete.</param>
    /// <param name="tableName">The name of the SQL table to modify.</param>
    /// <param name="idColumns">The columns used as identifiers in the table.</param>
    /// <param name="setColumns">The columns to be updated in the table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <remarks>
    /// This method assumes that the column names in the SQL table match the property names in the <see cref="StswObservableCollection{TModel}"/>.
    /// </remarks>
    [StswInfo("0.9.2", PlannedChanges = StswPlannedChanges.Remove)]
    public static void Set<TModel>(this StswDatabaseModel model, StswObservableCollection<TModel> items, string tableName, IEnumerable<string>? setColumns = null, IEnumerable<string>? idColumns = null, int? timeout = null, SqlTransaction? sqlTran = null) where TModel : IStswCollectionItem, new()
        => model.OpenedConnection().Set(items, tableName, setColumns, idColumns, model.DefaultTimeout ?? timeout, sqlTran);

    /// <summary>
    /// Builds a getter function for the specified property using expression trees.
    /// </summary>
    /// <param name="prop"> The property for which to build the getter.</param>
    /// <returns>A function that takes an object and returns the value of the specified property.</returns>
    [StswInfo("0.18.0", PlannedChanges = StswPlannedChanges.Remove)]
    private static Func<object, object?> BuildGetter(PropertyInfo prop)
    {
        var param = Expression.Parameter(typeof(object), "obj");
        var converted = Expression.Convert(param, prop.DeclaringType!);
        var propertyAccess = Expression.Property(converted, prop);
        var convertResult = Expression.Convert(propertyAccess, typeof(object));
        return Expression.Lambda<Func<object, object?>>(convertResult, param).Compile();
    }

    /// <summary>
    /// Checks if the query can be executed based on the current application state.
    /// </summary>
    /// <returns>
    /// Returns <see langword="false"/> if the application is in design mode and queries should not be executed, otherwise returns <see langword="true"/>.
    /// </returns>
    [StswInfo("0.9.3", "0.20.0")]
    private static bool CheckQueryConditions()
    {
        if (!StswDatabases.Config.IsEnabled)
            return false;

        if (StswDatabases.Config.ReturnIfInDesignMode && StswFn.IsInDesignMode)
            return false;

        return true;
    }

    /// <summary>
    /// Generates SQL parameters for the specified item based on the given column sets and item state.
    /// </summary>
    /// <typeparam name="TModel">The type of the item.</typeparam>
    /// <param name="item">The item to generate parameters for.</param>
    /// <param name="setColumns">The set of columns to be updated.</param>
    /// <param name="idColumns">The set of identifier columns.</param>
    /// <param name="commandType">The state of the item (Added, Modified, Deleted).</param>
    /// <returns>A collection of <see cref="SqlParameter"/> for the specified item.</returns>
    private static IEnumerable<SqlParameter> GenerateSqlParameters<TModel>(TModel item, IEnumerable<string> setColumns, IEnumerable<string> idColumns, StswItemState commandType) where TModel : IStswCollectionItem, new()
    {
        var setColumnsSet = new HashSet<string>(setColumns);
        var idColumnsSet = new HashSet<string>(idColumns);

        return typeof(TModel).GetProperties()
            .Where(x => (commandType == StswItemState.Added && setColumnsSet.Contains(x.Name)) ||
                        (commandType == StswItemState.Modified && (setColumnsSet.Contains(x.Name) || idColumnsSet.Contains(x.Name))) ||
                        (commandType == StswItemState.Deleted && idColumnsSet.Contains(x.Name)))
            .Select(x => new SqlParameter("@" + x.Name, x.GetValue(item) ?? DBNull.Value)
            { SqlDbType = x.PropertyType.InferSqlDbType() });
    }

    /// <summary>
    /// Trims trailing digits from a string, which is useful for normalizing column names that may have numeric suffixes.
    /// </summary>
    /// <param name="fullTable"> The string from which to trim trailing digits.</param>
    /// <param name="itemType"> The type of the item to which the column names belong.</param>
    /// <returns>A normalized string with trailing digits removed.</returns>
    [StswInfo("0.18.0", PlannedChanges = StswPlannedChanges.Remove)]
    private static Dictionary<string, string> GetColumnRenameMap(DataTable fullTable, Type itemType)
    {
        var itemProperties = itemType.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (DataColumn col in fullTable.Columns)
        {
            var baseName = TrimTrailingDigits(col.ColumnName);
            if (itemProperties.Contains(baseName) && !col.ColumnName.Equals(baseName, StringComparison.OrdinalIgnoreCase))
                map[col.ColumnName] = baseName;
        }

        return map;
    }

    /// <summary>
    /// Maps a CLR data type to the corresponding SQL data type.
    /// </summary>
    /// <param name="type">The CLR type to map.</param>
    /// <returns>A string representing the SQL data type.</returns>
    /// <exception cref="NotSupportedException">Thrown when the provided CLR type does not have a corresponding SQL data type.</exception>
    private static string GetSqlType(Type type) => type switch
    {
        _ when type == typeof(string) => "NVARCHAR(MAX)",
        _ when type == typeof(int) => "INT",
        _ when type == typeof(long) => "BIGINT",
        _ when type == typeof(decimal) => "DECIMAL(18, 2)",
        _ when type == typeof(double) => "FLOAT",
        _ when type == typeof(bool) => "BIT",
        _ when type == typeof(DateTime) => "DATETIME",
        _ => throw new NotSupportedException($"Type {type} is not supported.")
    };

    /// <summary>
    /// Collects property names suitable for insert/update (scalar-like, public get/set),
    /// skipping ItemState, indexers, collections (except byte[]), and attributes like NotMapped/ExcludeProperty.
    /// </summary>
    /// <param name="t">The type to inspect for writable scalar-like properties.</param>
    /// <returns>A list of property names that are writable and scalar-like.</returns>
    private static List<string> GetWritableScalarPropertyNames(Type t)
    {
        static bool HasAttribute(MemberInfo mi, string attrName)
            => mi.GetCustomAttributes(inherit: true).Any(a => string.Equals(a.GetType().Name, attrName, StringComparison.OrdinalIgnoreCase));

        var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var result = new List<string>();
        foreach (var p in props)
        {
            if (p.GetIndexParameters().Length > 0)
                continue;

            if (!(p.CanRead && p.CanWrite))
                continue;

            if (string.Equals(p.Name, nameof(IStswCollectionItem.ItemState), StringComparison.OrdinalIgnoreCase))
                continue;

            if (HasAttribute(p, "NotMappedAttribute"))
                continue;

            if (HasAttribute(p, "ExcludePropertyAttribute"))
                continue;

            var pt = p.PropertyType;
            var u = Nullable.GetUnderlyingType(pt) ?? pt;

            var isByteArray = pt == typeof(byte[]);
            var acceptable =
                u.IsPrimitive
                || u == typeof(string)
                || u == typeof(decimal)
                || u == typeof(DateTime)
                || u == typeof(Guid)
                || isByteArray;

            var isEnumerableButNotByteArray = typeof(IEnumerable).IsAssignableFrom(pt) && !isByteArray && pt != typeof(string);

            if (acceptable && !isEnumerableButNotByteArray)
                result.Add(p.Name);
        }

        return result;
    }

    /// <summary>
    /// Adds a parameter to the <see cref="SqlParameterCollection"/> with the specified name, value, type, and optional size.
    /// </summary>
    /// <param name="col">The <see cref="SqlParameterCollection"/> to which the parameter will be added.</param>
    /// <param name="name">The name of the parameter, including the '@' prefix.</param>
    /// <param name="value">The value of the parameter. If <see langword="null"/>, it will be set to <see cref="DBNull.Value"/>.</param>
    /// <param name="type">The SQL data type of the parameter.</param>
    /// <param name="size">The size of the parameter. Default is 0, which means no size is set.</param>
    /// <returns>The added <see cref="SqlParameter"/>.</returns>
    [StswInfo("0.21.0")]
    public static SqlParameter AddParam(this SqlParameterCollection col, string name, object? value, SqlDbType type, int size = 0)
    {
        var p = col.Add(name, type);
        if (size > 0) p.Size = size;
        p.Value = value ?? DBNull.Value;
        return p;
    }

    /// <summary>
    /// Reduces the amount of space in the given SQL query string by removing unnecessary whitespace 
    /// while preserving the content within string literals.
    /// </summary>
    /// <param name="query">The SQL query to process and reduce unnecessary whitespace.</param>
    /// <returns>The processed SQL query with reduced whitespace.</returns>
    [StswInfo("0.8.0")]
    public static string LessSpaceQuery(string query)
        => LessSpaceRegex().Matches(query)
                           .Cast<Match>()
                           .Select(x => x.Groups[2].Success ? (x.Groups[2].Value, true) : (x.Groups[3].Value, false))
                           .ToList()
                           .Aggregate(string.Empty, (current, part) => current + (
                               part.Item2
                                   ? $"'{part.Value}'"
                                   : StswFn.RemoveConsecutiveText(part.Value.Replace("\t", " "), " ")
                           ));

    /// <summary>
    /// Adds a list of parameters to the <see cref="SqlCommand"/> by replacing the specified parameter name in the SQL query with the list of values.
    /// If the list is null or empty, replaces the parameter with NULL in the SQL query.
    /// </summary>
    /// <param name="sqlCommand">The <see cref="SqlCommand"/> object.</param>
    /// <param name="parameterName">The parameter name to be replaced in the SQL query.</param>
    /// <param name="list">The list of values to be added as parameters.</param>
    /// <exception cref="ArgumentException">Thrown when the list contains more than 20 elements.</exception>
    [StswInfo("0.9.0")]
    public static void ParametersAddList(this SqlCommand sqlCommand, string parameterName, IList? list)
    {
        ArgumentNullException.ThrowIfNull(sqlCommand);
        if (string.IsNullOrEmpty(parameterName))
            throw new ArgumentException("Parameter name cannot be null or empty.", nameof(parameterName));

        const int maxListSize = 20;
        if (list?.Count > maxListSize)
            throw new ArgumentException($"The list contains more than {maxListSize} elements, which exceeds the allowed limit.", nameof(list));

        string replacementValue;
        if (list == null || list.Count == 0 || !list.GetType().IsListType(out var innerType) || innerType == null)
        {
            replacementValue = "NULL";
        }
        else
        {
            var sqlDbType = innerType.InferSqlDbType();
            replacementValue = string.Join(',', Enumerable.Range(0, list.Count).Select(i =>
            {
                var paramName = $"{parameterName}{i}";
                sqlCommand.Parameters.Add(paramName, sqlDbType).Value = list[i] ?? DBNull.Value;
                return paramName;
            }));
        }

        sqlCommand.CommandText = Regex.Replace(sqlCommand.CommandText, $@"{Regex.Escape(parameterName)}(?!\w)", replacementValue, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Prepares the specified SQL command by clearing existing parameters and adding new ones based on 
    /// the provided model. The model can be an IEnumerable of SQL parameters, a dictionary, or an object 
    /// whose properties will be used as parameters.
    /// </summary>
    /// <param name="sqlCommand">The SQL command to prepare with parameters.</param>
    /// <param name="parameterModel">The model containing the values to be added as parameters.</param>
    /// <param name="passAllParametersAnyway">If <see langword="true"/>, all parameters from the model will be added regardless of their usage in the query.</param>
    /// <returns>The prepared <see cref="SqlCommand"/>.</returns>
    [StswInfo("0.9.3", "0.20.0")]
    private static SqlCommand PrepareCommand(this SqlCommand sqlCommand, object? parameterModel, bool passAllParametersAnyway = false)
    {
        sqlCommand.Parameters.Clear();

        if (parameterModel == null)
            return sqlCommand;

        /// in query:
        /// remove everything between '' marks (including the quotes)
        /// remove everything between /* and */ (including the markers)
        /// remove everything between -- and newline (keeping the newline)
        /// and get all used parameters

        var query = SingleQuotesRegex().Replace(sqlCommand.CommandText, "");
        query = BlockCommentsRegex().Replace(query, "");
        query = LineCommentsRegex().Replace(query, "");

        var usedParameters = new HashSet<string>(
            ParameterRegex().Matches(query).Cast<Match>().Select(m => m.Groups[1].Value),
            StringComparer.OrdinalIgnoreCase
        );

        bool ShouldPass(string paramNameNoAt) => passAllParametersAnyway || usedParameters.Contains(paramNameNoAt);
        static string TrimAt(string n) => n?.TrimStart('@') ?? string.Empty;
        static string EnsureAt(string n) => n.StartsWith("@", StringComparison.Ordinal) ? n : "@" + n;

        switch (parameterModel)
        {
            case IEnumerable<SqlParameter> paramList:
                {
                    foreach (var p in paramList.Where(x => x is not null))
                    {
                        if (!ShouldPass(TrimAt(p.ParameterName)))
                            continue;

                        if (p.Value is IList list && p.Value is not byte[])
                            sqlCommand.ParametersAddList(p.ParameterName, list);
                        else
                            sqlCommand.Parameters.Add(p);
                    }
                    break;
                }

            case IDictionary<string, object?> dict:
                {
                    foreach (var kv in dict)
                    {
                        if (!ShouldPass(TrimAt(kv.Key)))
                            continue;

                        sqlCommand.PrepareParameter(EnsureAt(kv.Key), kv.Value);
                    }
                    break;
                }

            default:
                {
                    var t = parameterModel.GetType();
                    foreach (var pi in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (!ShouldPass(TrimAt(pi.Name)))
                            continue;

                        sqlCommand.PrepareParameter(EnsureAt(pi.Name), pi.GetValue(parameterModel));
                    }
                    break;
                }
        }

        return sqlCommand;
    }

    /// <summary>
    /// Prepares an INSERT SQL query for the specified model type and table name, optionally including a SCOPE_IDENTITY() retrieval.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to insert.</typeparam>
    /// <param name="items">The collection of items to be inserted.</param>
    /// <param name="tableName">The name of the table into which the items will be inserted.</param>
    /// <param name="withScopeIdentity">If <see langword="true"/>, the query will include a statement to retrieve the SCOPE_IDENTITY() after the insert.</param>
    /// <returns>The prepared INSERT SQL query string.</returns>
    [StswInfo("0.20.1", IsTested = false)]
    public static string PrepareInsertQuery<TModel>(IEnumerable<TModel> items, string tableName, bool withScopeIdentity)
    {
        var cols = GetWritableScalarPropertyNames(typeof(TModel));
        if (cols.Count == 0)
            throw new InvalidOperationException($"Type {typeof(TModel).Name} has no insertable properties.");

        var colList = string.Join(",", cols.Select(c => $"[{c}]"));
        var valList = string.Join(",", cols.Select(c => $"@{c}"));

        var scope = withScopeIdentity ? " ; SELECT CAST(SCOPE_IDENTITY() AS INT);" : ";";
        return $"INSERT INTO {tableName} ({colList}) VALUES ({valList}){scope}";
    }

    /// <summary>
    /// Prepares an UPDATE SQL query for the specified model type, table name, and WHERE clause.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to update.</typeparam>
    /// <param name="items">The collection of items to be updated.</param>
    /// <param name="tableName">The name of the table to update.</param>
    /// <param name="whereClause">The WHERE clause to specify which records to update. It should include parameter placeholders (e.g., @ParamName).</param>
    /// <returns>The prepared UPDATE SQL query string.</returns>
    [StswInfo("0.20.1", IsTested = false)]
    public static string PrepareUpdateQuery<TModel>(IEnumerable<TModel> items, string tableName, string whereClause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(whereClause);

        var allCols = GetWritableScalarPropertyNames(typeof(TModel));
        var usedInWhere = new HashSet<string>(
            ParameterRegex().Matches(whereClause).Cast<Match>().Select(m => m.Groups[1].Value),
            StringComparer.OrdinalIgnoreCase
        );

        var setCols = allCols.Where(c => !usedInWhere.Contains(c)).ToList();
        if (setCols.Count == 0)
            throw new InvalidOperationException("No updatable columns remain after excluding WHERE parameters.");

        var setClause = string.Join(",", setCols.Select(c => $"[{c}]=@{c}"));
        var where = whereClause.TrimStart().StartsWith("WHERE", StringComparison.OrdinalIgnoreCase)
            ? whereClause
            : "WHERE " + whereClause;

        return $"UPDATE {tableName} SET {setClause} {where};";
    }

    /// <summary>
    /// Prepares a single SQL parameter for the command, handling special cases like byte arrays and lists.
    /// </summary>
    /// <param name="sqlCommand">The SQL command to which the parameter will be added.</param>
    /// <param name="name">The name of the parameter, including the '@' prefix.</param>
    /// <param name="value">The value of the parameter to be added.</param>
    [StswInfo("0.20.0")]
    private static void PrepareParameter(this SqlCommand sqlCommand, string name, object? value)
    {
        if (value is byte[] bytes)
        {
            sqlCommand.Parameters.Add(name, SqlDbType.VarBinary, -1).Value = bytes;
            return;
        }

        if (value is IList list && value is not string)
        {
            sqlCommand.ParametersAddList(name, list);
            return;
        }

        sqlCommand.Parameters.Add(new SqlParameter(name, value ?? DBNull.Value));
    }

    /// <summary>
    /// Prepares the SQL query for execution by optionally removing unnecessary whitespace.
    /// </summary>
    /// <param name="query">The SQL query to prepare.</param>
    /// <returns>The prepared SQL query.</returns>
    private static string PrepareQuery(string query) => StswDatabases.Config.MakeLessSpaceQuery ? LessSpaceQuery(query) : query;

    /// <summary>
    /// Trims trailing digits from the end of a string.
    /// </summary>
    /// <param name="name"> The string from which to trim trailing digits.</param>
    /// <returns>The string with trailing digits removed.</returns>
    [StswInfo("0.18.0")]
    private static string TrimTrailingDigits(string name)
    {
        var i = name.Length - 1;
        while (i >= 0 && char.IsDigit(name[i])) i--;
        return name[..(i + 1)];
    }
}
