using Microsoft.Data.SqlClient;
using System.Collections;
using System.Data;

namespace StswExpress.Commons;

public static partial class StswDatabaseHelper
{
    /// <summary>
    /// Asynchronously opens the SQL connection if it is not already open.
    /// </summary>
    /// <param name="connection">The SQL connection to open.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The opened <see cref="SqlConnection"/>.</returns>
    public static async Task<SqlConnection> GetOpenedAsync(this SqlConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return connection;
    }

    /// <summary>
    /// Asynchronously opens a new SQL connection using the connection string from the provided <see cref="StswDatabaseModel"/>.
    /// </summary>
    /// <param name="model">The database model containing the connection information.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The opened <see cref="SqlConnection"/>.</returns>
    public static async Task<SqlConnection> OpenedConnectionAsync(this StswDatabaseModel model, CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(model.GetConnString());
        try
        {
            return await connection.GetOpenedAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Asynchronously performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    public static async Task BulkInsertAsync<TModel>(
        this SqlConnection sqlConn,
        IEnumerable<TModel> items,
        string tableName,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        bool? disposeConnection = null,
        CancellationToken cancellationToken = default)
    {
        if (typeof(IDictionary<string, object?>).IsAssignableFrom(typeof(TModel)))
        {
            await sqlConn.BulkInsertAsync((IEnumerable)items, tableName, timeout, sqlTran, disposeConnection, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!CheckQueryConditions())
            return;

        using var factory = await StswSqlConnectionFactory.CreateAsync(sqlConn, sqlTran, true, disposeConnection, cancellationToken).ConfigureAwait(false);
        using var sqlBulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        sqlBulkCopy.BulkCopyTimeout = timeout ?? sqlBulkCopy.BulkCopyTimeout;
        sqlBulkCopy.DestinationTableName = tableName;

        var dt = items.ToDataTable();
        foreach (DataColumn column in dt.Columns)
            sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        await sqlBulkCopy.WriteToServerAsync(dt, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        factory.Commit();
    }

    /// <summary>
    /// Asynchronously performs a bulk insert operation within the specified transaction.
    /// </summary>
    public static Task BulkInsertAsync<TModel>(
        this SqlTransaction sqlTran,
        IEnumerable<TModel> items,
        string tableName,
        int? timeout = null,
        CancellationToken cancellationToken = default)
        => sqlTran.Connection.BulkInsertAsync(items, tableName, timeout, sqlTran, cancellationToken: cancellationToken);

    /// <summary>
    /// Asynchronously performs a bulk insert operation using the provided database model.
    /// </summary>
    public static Task BulkInsertAsync<TModel>(
        this StswDatabaseModel model,
        IEnumerable<TModel> items,
        string tableName,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        CancellationToken cancellationToken = default)
    {
        var connection = GetConnection(model, sqlTran);
        return connection.BulkInsertAsync(items, tableName, model.DefaultTimeout ?? timeout, sqlTran, sqlTran == null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    public static async Task BulkInsertAsync(
        this SqlConnection sqlConn,
        IEnumerable items,
        string tableName,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        bool? disposeConnection = null,
        CancellationToken cancellationToken = default)
    {
        if (!CheckQueryConditions())
            return;

        using var factory = await StswSqlConnectionFactory.CreateAsync(sqlConn, sqlTran, true, disposeConnection, cancellationToken).ConfigureAwait(false);
        using var sqlBulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        sqlBulkCopy.BulkCopyTimeout = timeout ?? sqlBulkCopy.BulkCopyTimeout;
        sqlBulkCopy.DestinationTableName = tableName;

        var dt = items.ToDataTable();
        if (dt.Columns.Count == 0)
            throw new InvalidOperationException("Cannot infer column definitions from the provided items.");

        foreach (DataColumn column in dt.Columns)
            sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        await sqlBulkCopy.WriteToServerAsync(dt, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        factory.Commit();
    }

    /// <summary>
    /// Asynchronously performs a bulk insert operation within the specified transaction.
    /// </summary>
    public static Task BulkInsertAsync(
        this SqlTransaction sqlTran,
        IEnumerable items,
        string tableName,
        int? timeout = null,
        CancellationToken cancellationToken = default)
        => sqlTran.Connection.BulkInsertAsync(items, tableName, timeout, sqlTran, cancellationToken: cancellationToken);

    /// <summary>
    /// Asynchronously performs a bulk insert operation using the provided database model.
    /// </summary>
    public static Task BulkInsertAsync(
        this StswDatabaseModel model,
        IEnumerable items,
        string tableName,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        CancellationToken cancellationToken = default)
    {
        var connection = GetConnection(model, sqlTran);
        return connection.BulkInsertAsync(items, tableName, model.DefaultTimeout ?? timeout, sqlTran, sqlTran == null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes a non-query SQL command and returns the number of rows affected.
    /// </summary>
    public static async Task<int?> ExecuteNonQueryAsync(
        this SqlConnection sqlConn,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        bool? disposeConnection = null,
        CancellationToken cancellationToken = default)
    {
        if (!CheckQueryConditions())
            return default;

        var parameterModels = parameters switch
        {
            IEnumerable<SqlParameter> => new List<object?> { parameters },
            IEnumerable<object?> enumerable => enumerable.ToList(),
            _ => new List<object?> { parameters },
        };

        using var factory = await StswSqlConnectionFactory.CreateAsync(sqlConn, sqlTran, parameterModels.Count > 1, disposeConnection, cancellationToken).ConfigureAwait(false);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;

        var result = 0;
        foreach (var parameterModel in parameterModels)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result += await sqlCmd.PrepareCommand(parameterModel).ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        factory.Commit();
        return result;
    }

    /// <summary>
    /// Asynchronously executes a non-query SQL command within the specified transaction.
    /// </summary>
    public static Task<int?> ExecuteNonQueryAsync(
        this SqlTransaction sqlTran,
        string query,
        object? parameters = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
        => sqlTran.Connection.ExecuteNonQueryAsync(query, parameters, timeout, sqlTran, cancellationToken: cancellationToken);

    /// <summary>
    /// Asynchronously executes a non-query SQL command using the provided database model.
    /// </summary>
    public static Task<int?> ExecuteNonQueryAsync(
        this StswDatabaseModel model,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        CancellationToken cancellationToken = default)
    {
        var connection = GetConnection(model, sqlTran);
        return connection.ExecuteNonQueryAsync(query, parameters, model.DefaultTimeout ?? timeout, sqlTran, sqlTran == null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes a SQL query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// The returned reader owns the connection when no external transaction is supplied and closes it when disposed.
    /// </summary>
    public static async Task<SqlDataReader> ExecuteReaderAsync(
        this SqlConnection sqlConn,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        CancellationToken cancellationToken = default)
    {
        if (!CheckQueryConditions())
            return default!;

        using var factory = await StswSqlConnectionFactory.CreateAsync(sqlConn, sqlTran, false, false, cancellationToken).ConfigureAwait(false);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        sqlCmd.PrepareCommand(parameters);

        var behavior = factory.Transaction != null ? CommandBehavior.Default : CommandBehavior.CloseConnection;
        return await sqlCmd.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes a SQL query within the specified transaction and returns a data reader.
    /// </summary>
    public static Task<SqlDataReader> ExecuteReaderAsync(
        this SqlTransaction sqlTran,
        string query,
        object? parameters = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
        => sqlTran.Connection.ExecuteReaderAsync(query, parameters, timeout, sqlTran, cancellationToken);

    /// <summary>
    /// Asynchronously executes a SQL query using the provided database model and returns a data reader.
    /// </summary>
    public static Task<SqlDataReader> ExecuteReaderAsync(
        this StswDatabaseModel model,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        CancellationToken cancellationToken = default)
    {
        var connection = GetConnection(model, sqlTran);
        return connection.ExecuteReaderAsync(query, parameters, model.DefaultTimeout ?? timeout, sqlTran, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes a SQL query and returns a scalar value.
    /// </summary>
    public static async Task<TResult> ExecuteScalarAsync<TResult>(
        this SqlConnection sqlConn,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        bool? disposeConnection = null,
        CancellationToken cancellationToken = default)
    {
        if (!CheckQueryConditions())
            return default!;

        using var factory = await StswSqlConnectionFactory.CreateAsync(sqlConn, sqlTran, false, disposeConnection, cancellationToken).ConfigureAwait(false);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;

        var rawResult = await sqlCmd.PrepareCommand(parameters).ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result = rawResult.ConvertTo<TResult?>();

        factory.Commit();
        return result!;
    }

    /// <summary>
    /// Asynchronously executes a SQL query within the specified transaction and returns a scalar value.
    /// </summary>
    public static Task<TResult> ExecuteScalarAsync<TResult>(
        this SqlTransaction sqlTran,
        string query,
        object? parameters = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
        => sqlTran.Connection.ExecuteScalarAsync<TResult>(query, parameters, timeout, sqlTran, cancellationToken: cancellationToken);

    /// <summary>
    /// Asynchronously executes a SQL query using the provided database model and returns a scalar value.
    /// </summary>
    public static Task<TResult> ExecuteScalarAsync<TResult>(
        this StswDatabaseModel model,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        CancellationToken cancellationToken = default)
    {
        var connection = GetConnection(model, sqlTran);
        return connection.ExecuteScalarAsync<TResult>(query, parameters, model.DefaultTimeout ?? timeout, sqlTran, sqlTran == null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes a stored procedure with parameters and returns the number of rows affected.
    /// </summary>
    public static async Task<int?> ExecuteStoredProcedureAsync(
        this SqlConnection sqlConn,
        string procName,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        bool? disposeConnection = null,
        CancellationToken cancellationToken = default)
    {
        if (!CheckQueryConditions())
            return default;

        using var factory = await StswSqlConnectionFactory.CreateAsync(sqlConn, sqlTran, true, disposeConnection, cancellationToken).ConfigureAwait(false);
        using var sqlCmd = new SqlCommand(procName, factory.Connection, factory.Transaction)
        {
            CommandType = CommandType.StoredProcedure,
        };
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;

        var result = await sqlCmd.PrepareCommand(parameters, passAllParametersAnyway: true)
            .ExecuteNonQueryAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        factory.Commit();
        return result;
    }

    /// <summary>
    /// Asynchronously executes a stored procedure within the specified transaction.
    /// </summary>
    public static Task<int?> ExecuteStoredProcedureAsync(
        this SqlTransaction sqlTran,
        string procName,
        object? parameters = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
        => sqlTran.Connection.ExecuteStoredProcedureAsync(procName, parameters, timeout, sqlTran, cancellationToken: cancellationToken);

    /// <summary>
    /// Asynchronously executes a stored procedure using the provided database model.
    /// </summary>
    public static Task<int?> ExecuteStoredProcedureAsync(
        this StswDatabaseModel model,
        string procName,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        CancellationToken cancellationToken = default)
    {
        var connection = GetConnection(model, sqlTran);
        return connection.ExecuteStoredProcedureAsync(procName, parameters, model.DefaultTimeout ?? timeout, sqlTran, sqlTran == null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes a SQL query and returns a collection of mapped results.
    /// </summary>
    public static async Task<IEnumerable<TResult>> GetAsync<TResult>(
        this SqlConnection sqlConn,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        bool? disposeConnection = null,
        CancellationToken cancellationToken = default)
    {
        if (!CheckQueryConditions())
            return [];

        var dt = await ExecuteDataTableAsync(sqlConn, query, parameters, timeout, sqlTran, disposeConnection, cancellationToken).ConfigureAwait(false);
        var delimiter = StswDatabases.Config.DelimiterForMapping;
        var hasDelimiter = dt.Columns.Cast<DataColumn>().Any(col => col.ColumnName.Contains(delimiter));

        return hasDelimiter
            ? dt.MapTo<TResult>(delimiter)
            : dt.MapTo<TResult>();
    }

    /// <summary>
    /// Asynchronously executes a SQL query within the specified transaction and returns mapped results.
    /// </summary>
    public static Task<IEnumerable<TResult>> GetAsync<TResult>(
        this SqlTransaction sqlTran,
        string query,
        object? parameters = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
        => sqlTran.Connection.GetAsync<TResult>(query, parameters, timeout, sqlTran, cancellationToken: cancellationToken);

    /// <summary>
    /// Asynchronously executes a SQL query using the provided database model and returns mapped results.
    /// </summary>
    public static Task<IEnumerable<TResult>> GetAsync<TResult>(
        this StswDatabaseModel model,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        CancellationToken cancellationToken = default)
    {
        var connection = GetConnection(model, sqlTran);
        return connection.GetAsync<TResult>(query, parameters, model.DefaultTimeout ?? timeout, sqlTran, sqlTran == null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes a SQL query and returns a collection of mapped results of the supplied runtime type.
    /// </summary>
    public static async Task<IEnumerable<object?>> GetAsync(
        this SqlConnection sqlConn,
        Type type,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        bool? disposeConnection = null,
        CancellationToken cancellationToken = default)
    {
        if (!CheckQueryConditions())
            return [];

        var dt = await ExecuteDataTableAsync(sqlConn, query, parameters, timeout, sqlTran, disposeConnection, cancellationToken).ConfigureAwait(false);
        var delimiter = StswDatabases.Config.DelimiterForMapping;
        var hasDelimiter = dt.Columns.Cast<DataColumn>().Any(col => col.ColumnName.Contains(delimiter));

        return hasDelimiter
            ? dt.MapTo(type, delimiter).Cast<object?>()
            : dt.MapTo(type).Cast<object?>();
    }

    /// <summary>
    /// Asynchronously executes a SQL query within the specified transaction and returns results of the supplied runtime type.
    /// </summary>
    public static Task<IEnumerable<object?>> GetAsync(
        this SqlTransaction sqlTran,
        Type type,
        string query,
        object? parameters = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
        => sqlTran.Connection.GetAsync(type, query, parameters, timeout, sqlTran, cancellationToken: cancellationToken);

    /// <summary>
    /// Asynchronously executes a SQL query using the provided database model and returns results of the supplied runtime type.
    /// </summary>
    public static Task<IEnumerable<object?>> GetAsync(
        this StswDatabaseModel model,
        Type type,
        string query,
        object? parameters = null,
        int? timeout = null,
        SqlTransaction? sqlTran = null,
        CancellationToken cancellationToken = default)
    {
        var connection = GetConnection(model, sqlTran);
        return connection.GetAsync(type, query, parameters, model.DefaultTimeout ?? timeout, sqlTran, sqlTran == null, cancellationToken);
    }

    private static SqlConnection GetConnection(StswDatabaseModel model, SqlTransaction? sqlTran)
        => sqlTran != null
            ? sqlTran.Connection ?? throw new InvalidOperationException("External transaction is not associated with any connection.")
            : new SqlConnection(model.GetConnString());

    private static async Task<DataTable> ExecuteDataTableAsync(
        SqlConnection sqlConn,
        string query,
        object? parameters,
        int? timeout,
        SqlTransaction? sqlTran,
        bool? disposeConnection,
        CancellationToken cancellationToken)
    {
        using var factory = await StswSqlConnectionFactory.CreateAsync(sqlConn, sqlTran, false, disposeConnection, cancellationToken).ConfigureAwait(false);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        sqlCmd.PrepareCommand(parameters);

        using var reader = await sqlCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var dt = CreateDataTableSchema(reader);
        var values = new object[reader.FieldCount];

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            reader.GetValues(values);
            dt.Rows.Add((object[])values.Clone());
        }

        cancellationToken.ThrowIfCancellationRequested();
        return dt;
    }

    private static DataTable CreateDataTableSchema(SqlDataReader reader)
    {
        var dt = new DataTable();
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var baseName = reader.GetName(i);
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = $"Column{i + 1}";

            var columnName = baseName;
            var suffix = 1;
            while (!usedNames.Add(columnName))
                columnName = baseName + suffix++;

            var fieldType = reader.GetFieldType(i);
            dt.Columns.Add(columnName, fieldType);
        }

        return dt;
    }
}
