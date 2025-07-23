using Microsoft.Data.SqlClient;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StswExpress.Commons;

/// <summary>
/// A static helper class that provides extension methods for performing common database operations 
/// such as bulk inserts, executing queries, and handling stored procedures in a more efficient manner.
/// </summary>
public static class StswDatabaseHelper
{
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
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    public static void BulkInsert<TModel>(this SqlConnection sqlConn, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return;
        
        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, disposeConnection);

        using var sqlBulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        sqlBulkCopy.BulkCopyTimeout = timeout ?? sqlBulkCopy.BulkCopyTimeout;
        sqlBulkCopy.DestinationTableName = tableName;

        var dt = items.ToDataTable();
        foreach (DataColumn column in dt.Columns)
            sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
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
    public static void BulkInsert<TModel>(this StswDatabaseModel model, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().BulkInsert(items, tableName, timeout, sqlTran);

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
    public static int? ExecuteNonQuery(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteNonQuery(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A <see cref="SqlDataReader"/> instance for reading the data, or null if the query conditions are not met.</returns>
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
    public static SqlDataReader ExecuteReader(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteReader(query, parameters, timeout, sqlTran);

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
    public static TResult? ExecuteScalar<TResult>(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return default;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false, disposeConnection);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        var result = sqlCmd.PrepareCommand(parameters).ExecuteScalar().ConvertTo<TResult?>();

        factory.Commit();
        return result;
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
    public static TResult? ExecuteScalar<TResult>(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
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
    public static TResult? ExecuteScalar<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteScalar<TResult>(query, parameters, timeout, sqlTran);

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
    public static int? ExecuteStoredProcedure(this StswDatabaseModel model, string procName, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteStoredProcedure(procName, parameters, timeout, sqlTran);

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
    public static IEnumerable<TResult?> Get<TResult>(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
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
    public static IEnumerable<TResult?> Get<TResult>(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.Get<TResult?>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">Optional. The command timeout value in seconds. If <see langword="null"/>, the default timeout is used.</param>
    /// <param name="sqlTran">Optional. The SQL transaction to use for this operation. If <see langword="null"/>, no transaction is used.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    public static IEnumerable<TResult?> Get<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().Get<TResult?>(query, parameters, timeout, sqlTran);

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
    public static IEnumerable<object?> Get(this StswDatabaseModel model, Type type, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().Get(type, query, parameters, timeout, sqlTran);

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
    public static IEnumerable<THeader> GetDivided<THeader, TItem>(this StswDatabaseModel model, string query, KeyValuePair<string, string?> joinKeys, string injectIntoProperty, string divideFromColumn, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().GetDivided<THeader, TItem>(query, joinKeys, injectIntoProperty, divideFromColumn, parameters, timeout, sqlTran);

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
    public static void TempTableInsert<TModel>(this SqlConnection sqlConn, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
    {
        if (!CheckQueryConditions())
            return;

        var dt = items.ToDataTable();
        tableName = tableName.Trim('#');

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, false);
        using var sqlCmd = new SqlCommand(GenerateCreateTableScript(dt, tableName), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        sqlCmd.ExecuteNonQuery();
        
        using var sqlBulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        sqlBulkCopy.BulkCopyTimeout = timeout ?? sqlBulkCopy.BulkCopyTimeout;
        sqlBulkCopy.DestinationTableName = "#" + tableName;

        foreach (DataColumn column in dt.Columns)
            sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
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
    public static void TempTableInsert<TModel>(this StswDatabaseModel model, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().TempTableInsert(items, tableName, timeout, sqlTran);

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
    public static void Set<TModel>(this StswDatabaseModel model, StswObservableCollection<TModel> items, string tableName, IEnumerable<string>? setColumns = null, IEnumerable<string>? idColumns = null, int? timeout = null, SqlTransaction? sqlTran = null) where TModel : IStswCollectionItem, new()
        => model.OpenedConnection().Set(items, tableName, setColumns, idColumns, timeout, sqlTran);

    /// <summary>
    /// Checks if the query can be executed based on the current application state.
    /// </summary>
    /// <returns>
    /// Returns <see langword="false"/> if the application is in design mode and queries should not be executed, otherwise returns <see langword="true"/>.
    /// </returns>
    public static bool CheckQueryConditions()
    {
        if (!StswDatabases.Config.IsEnabled)
            return false;

        if (StswDatabases.Config.ReturnIfInDesignerMode && IsInDesignMode())
            return false;

        return true;
    }

    /// <summary>
    /// Determines if the application is in design mode.
    /// </summary>
    /// <returns>True if in design mode, otherwise false.</returns>
    private static bool IsInDesignMode()
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || IsDesignerProcess())
            return true;

        if (StswFn.IsUiThreadAvailable())
        {
            var isInDesignMode = false;
            SynchronizationContext.Current?.Send(_ => isInDesignMode = CheckDesignerEnvironment(), null);
            return isInDesignMode;
        }

        return false;
    }

    /// <summary>
    /// Checks if the current process is running inside a designer.
    /// </summary>
    /// <returns>True if running inside a designer, otherwise false.</returns>
    private static bool IsDesignerProcess()
    {
        var processName = Process.GetCurrentProcess().ProcessName;
        return processName.Contains("devenv") || processName.Contains("Blend");
    }

    /// <summary>
    /// Placeholder method for additional design-time detection logic.
    /// </summary>
    /// <returns>True if the environment is detected as a designer.</returns>
    private static bool CheckDesignerEnvironment()
    {
        var processName = Process.GetCurrentProcess().ProcessName.ToLower();

        if (processName.Contains("xdesproc"))
            return true;

        return false;
    }

    /// <summary>
    /// Builds a getter function for the specified property using expression trees.
    /// </summary>
    /// <param name="prop"> The property for which to build the getter.</param>
    /// <returns>A function that takes an object and returns the value of the specified property.</returns>
    private static Func<object, object?> BuildGetter(PropertyInfo prop)
    {
        var param = Expression.Parameter(typeof(object), "obj");
        var converted = Expression.Convert(param, prop.DeclaringType!);
        var propertyAccess = Expression.Property(converted, prop);
        var convertResult = Expression.Convert(propertyAccess, typeof(object));
        return Expression.Lambda<Func<object, object?>>(convertResult, param).Compile();
    }

    /// <summary>
    /// Generates the SQL script to create a temporary table based on the structure of the provided DataTable.
    /// </summary>
    /// <param name="dt">The DataTable that defines the structure of the table to be created.</param>
    /// <param name="tableName">The name of the temporary table to be created.</param>
    /// <returns>A SQL script string for creating the temporary table.</returns>
    private static string GenerateCreateTableScript(DataTable dt, string tableName)
    {
        var columnsDefinitions = dt.Columns
            .Cast<DataColumn>()
            .Select(col => $"[{col.ColumnName}] {GetSqlType(col.DataType)}");

        return $"CREATE TABLE #{tableName} ({string.Join(", ", columnsDefinitions)});";
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
    /// Reduces the amount of space in the given SQL query string by removing unnecessary whitespace 
    /// while preserving the content within string literals.
    /// </summary>
    /// <param name="query">The SQL query to process and reduce unnecessary whitespace.</param>
    /// <returns>The processed SQL query with reduced whitespace.</returns>
    public static string LessSpaceQuery(string query)
        => new Regex(@"('([^']*)')|([^']+)").Matches(query)
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
    /// <returns>The prepared <see cref="SqlCommand"/>.</returns>
    private static SqlCommand PrepareCommand(this SqlCommand sqlCommand, object? parameterModel, bool passAllParametersAnyway = false)
    {
        sqlCommand.Parameters.Clear();

        if (parameterModel != null)
        {
            /// in query:
            /// remove everything between '' marks (including the quotes)
            /// remove everything between /* and */ (including the markers)
            /// remove everything between -- and newline (keeping the newline)
            /// and get all used parameters
            
            var query = Regex.Replace(sqlCommand.CommandText, @"'[^']*'", "");
            query = Regex.Replace(query, @"/\*.*?\*/", "", RegexOptions.Singleline);
            query = Regex.Replace(query, @"--.*?$", "", RegexOptions.Multiline);

            var cmdParameters = Regex.Matches(query, @"@(\w+)").Cast<Match>().Select(x => x.Groups[1].Value.ToLower());

            /// prepare parameters from model
            var sqlParameters = (parameterModel switch
            {
                IEnumerable<SqlParameter> paramList => paramList,
                IDictionary<string, object> dict => dict.Where(x => x.Key.ToLower().In(cmdParameters))
                                                        .Select(x => new SqlParameter("@" + x.Key, x.Value ?? DBNull.Value)),
                _ => parameterModel.GetType()
                          .GetProperties()
                          .Where(x => passAllParametersAnyway || x.Name.ToLower().In(cmdParameters))
                          .Select(x => new SqlParameter("@" + x.Name, x.GetValue(parameterModel) ?? DBNull.Value))
            }).ToList();
            
            /// add prepared parameters to command
            foreach (var sqlParameter in sqlParameters)
            {
                if (sqlParameter.Value?.GetType()?.IsListType(out var type) == true)
                {
                    if (type == typeof(byte))
                        sqlCommand.Parameters.Add(sqlParameter.ParameterName, SqlDbType.VarBinary, -1).Value = sqlParameter.Value;
                    else if (type?.IsValueType == true)
                        sqlCommand.ParametersAddList(sqlParameter.ParameterName, (IList?)sqlParameter.Value);
                }
                else
                {
                    sqlCommand.Parameters.Add(sqlParameter);
                }
            }
        }

        return sqlCommand;
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
    private static string TrimTrailingDigits(string name)
    {
        var i = name.Length - 1;
        while (i >= 0 && char.IsDigit(name[i])) i--;
        return name[..(i + 1)];
    }
}
