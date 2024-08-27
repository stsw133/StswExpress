using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace StswExpress;

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
    public static SqlConnection Opened(this SqlConnection connection)
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
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    public static void BulkInsert<TModel>(this SqlConnection sqlConn, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, disposeConnection);

        using var bulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        bulkCopy.BulkCopyTimeout = timeout ?? bulkCopy.BulkCopyTimeout;
        bulkCopy.DestinationTableName = tableName;

        var dataTable = items.ToDataTable();
        bulkCopy.WriteToServer(dataTable);

        factory.Commit();
    }

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    public static void BulkInsert<TModel>(this SqlTransaction sqlTran, IEnumerable<TModel> items, string tableName, int? timeout = null)
        => sqlTran.Connection.BulkInsert(items, tableName, timeout, sqlTran);

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    public static void BulkInsert<TModel>(this StswDatabaseModel model, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().BulkInsert(items, tableName, timeout, sqlTran);

    /// <summary>
    /// Executes a non-query SQL command and returns the number of rows affected.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
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
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    public static int? ExecuteNonQuery(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteNonQuery(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a non-query SQL command and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    public static int? ExecuteNonQuery(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteNonQuery(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>A <see cref="SqlDataReader"/> instance for reading the data, or null if the query conditions are not met.</returns>
    public static SqlDataReader? ExecuteReader(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return default;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false, disposeConnection);
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
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>A <see cref="SqlDataReader"/> instance for reading the data, or null if the query conditions are not met.</returns>
    public static SqlDataReader? ExecuteReader(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteReader(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>A <see cref="SqlDataReader"/> instance for reading the data, or null if the query conditions are not met.</returns>
    public static SqlDataReader? ExecuteReader(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteReader(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
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
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>The scalar value, or null if the query conditions are not met.</returns>
    public static TResult? ExecuteScalar<TResult>(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteScalar<TResult>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>The scalar value, or null if the query conditions are not met.</returns>
    public static TResult? ExecuteScalar<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteScalar<TResult>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a stored procedure with parameters and returns the number of rows affected.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="procName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">The model used for the stored procedure parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    public static int? ExecuteStoredProcedure(this SqlConnection sqlConn, string procName, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
    {
        if (!CheckQueryConditions())
            return default;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, disposeConnection);
        using var sqlCmd = new SqlCommand(procName, factory.Connection, factory.Transaction) { CommandType = CommandType.StoredProcedure, };
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        var result = sqlCmd.PrepareCommand(parameters).ExecuteNonQuery();
        factory.Commit();
        return result;
    }

    /// <summary>
    /// Executes a stored procedure with parameters and returns the number of rows affected.
    /// </summary>
    /// <param name="procName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">The model used for the stored procedure parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>The number of rows affected, or null if the query conditions are not met.</returns>
    public static int? ExecuteStoredProcedure(this SqlTransaction sqlTran, string procName, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteStoredProcedure(procName, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a stored procedure with parameters and returns the number of rows affected.
    /// </summary>
    /// <param name="procName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">The model used for the stored procedure parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
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
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    public static IEnumerable<TResult> Get<TResult>(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null) where TResult : class, new()
    {
        if (!CheckQueryConditions())
            return [];

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false, disposeConnection);
        using var sqlDA = new SqlDataAdapter(PrepareQuery(query), factory.Connection);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? sqlDA.SelectCommand.CommandTimeout;
        sqlDA.SelectCommand.Transaction = factory.Transaction;
        sqlDA.SelectCommand.PrepareCommand(parameters);

        var dataTable = new DataTable();
        sqlDA.Fill(dataTable);

        return dataTable.MapTo<TResult>(StswDatabases.Config.DelimiterForMapping);
    }

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    public static IEnumerable<TResult> Get<TResult>(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null) where TResult : class, new()
        => sqlTran.Connection.Get<TResult>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    public static IEnumerable<TResult> Get<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null) where TResult : class, new()
        => model.OpenedConnection().Get<TResult>(query, parameters, timeout, sqlTran);

    /*
    /// <summary>
    /// Executes a multi-select SQL query and returns a collection of results, where each result set is mapped to a specified type.
    /// </summary>
    /// <param name="model">The database model containing connection and transaction information.</param>
    /// <param name="query">The SQL query string containing multiple SELECT statements, separated by semicolons.</param>
    /// <param name="resultTypes">A list of types corresponding to the result sets returned by each SELECT statement in the query.</param>
    /// <param name="parameters">Optional parameters to be used in the SQL query.</param>
    /// <param name="timeout">The timeout used for the SQL command. If not specified, the default timeout is used.</param>
    /// <param name="delimiter">An optional delimiter used for mapping nested properties in the result sets. Defaults to '/' if not specified.</param>
    /// <returns>
    /// A collection of collections, where each inner collection contains objects mapped to the specified type corresponding to each SELECT statement.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the number of result types does not match the number of SELECT statements in the SQL command.</exception>
    public static IEnumerable<IEnumerable<object>> GetMultiple(this StswDatabaseModel model, string query, IList<Type> resultTypes, object? parameters = null, int? timeout = null, SqlTransaction? externalTransaction = null, char delimiter = '/')
    {
        if (!CheckQueryConditions())
            return [];

        using var factory = new StswSqlConnectionFactory(model, externalTransaction, false);
        using var sqlDA = new SqlDataAdapter(model.PrepareQuery(query), factory.Connection);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlDA.SelectCommand.CommandTimeout;
        sqlDA.SelectCommand.Transaction = factory.Transaction;

        PrepareParameters(sqlDA.SelectCommand, parameters);

        var dataSet = new DataSet();
        sqlDA.Fill(dataSet);

        if (dataSet.Tables.Count != resultTypes.Count)
            throw new InvalidOperationException("The number of result types does not match the number of queries in the SQL command.");

        var results = new List<IEnumerable<object>>();

        for (int i = 0; i < dataSet.Tables.Count; i++)
        {
            var table = dataSet.Tables[i];
            var resultType = resultTypes[i];

            var mapMethod = typeof(DataTableExtensions).GetMethod("MapTo", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(resultType);
            if (mapMethod != null)
            {
                var mappedResult = mapMethod.Invoke(null, [table, delimiter]);
                if (mappedResult != null)
                    results.Add((IEnumerable<object>)mappedResult);
            }
        }

        factory.Commit();
        return results;
    }
    */

    /// <summary>
    /// Performs insert, update, and delete operations on a SQL table based on the state of the items in the provided <see cref="StswBindingList{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the items in the list.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="items">The list of items to insert, update, or delete.</param>
    /// <param name="tableName">The name of the SQL table to modify.</param>
    /// <param name="setColumns">The columns to be updated in the table.</param>
    /// <param name="idColumns">The columns used as identifiers in the table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <remarks>
    /// This method assumes that the column names in the SQL table match the property names in the <see cref="StswBindingList{TModel}"/>.
    /// </remarks>
    public static void Set<TModel>(this SqlConnection sqlConn, StswBindingList<TModel> items, string tableName, IEnumerable<string>? setColumns = null, IEnumerable<string>? idColumns = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null) where TModel : IStswCollectionItem, new()
    {
        if (!CheckQueryConditions())
            return;

        idColumns ??= ["ID"];
        setColumns ??= typeof(TModel).GetProperties().Select(x => x.Name);
        setColumns = setColumns.Except(items.IgnoredProperties);

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true, disposeConnection);

        var insertQuery = $"insert into {tableName} ({string.Join(',', setColumns)}) values ({string.Join(',', setColumns.Select(x => "@" + x))})";
        using (var sqlCmd = new SqlCommand(PrepareQuery(insertQuery), factory.Connection, factory.Transaction))
        {
            sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
            foreach (var item in items.GetItemsByState(StswItemState.Added))
                sqlCmd.PrepareCommand(GenerateSqlParameters(item, setColumns, idColumns, item.ItemState)).ExecuteNonQuery();
        }
        
        var updateQuery = $"update {tableName} set {string.Join(',', setColumns.Select(x => x + "=@" + x))} where {string.Join(',', idColumns.Select(x => x + "=@" + x))}";
        using (var sqlCmd = new SqlCommand(PrepareQuery(updateQuery), factory.Connection, factory.Transaction))
        {
            sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
            foreach (var item in items.GetItemsByState(StswItemState.Modified))
                sqlCmd.PrepareCommand(GenerateSqlParameters(item, setColumns, idColumns, item.ItemState)).ExecuteNonQuery();
        }
        
        var deleteQuery = $"delete from {tableName} where {string.Join(',', idColumns.Select(x => x + "=@" + x))}";
        using (var sqlCmd = new SqlCommand(PrepareQuery(deleteQuery), factory.Connection, factory.Transaction))
        {
            sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
            foreach (var item in items.GetItemsByState(StswItemState.Deleted))
                PrepareCommand(sqlCmd, GenerateSqlParameters(item, setColumns, idColumns, item.ItemState)).ExecuteNonQuery();
        }

        factory.Commit();
    }

    /// <summary>
    /// Performs insert, update, and delete operations on a SQL table based on the state of the items in the provided <see cref="StswBindingList{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the items in the list.</typeparam>
    /// <param name="items">The list of items to insert, update, or delete.</param>
    /// <param name="tableName">The name of the SQL table to modify.</param>
    /// <param name="setColumns">The columns to be updated in the table.</param>
    /// <param name="idColumns">The columns used as identifiers in the table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <remarks>
    /// This method assumes that the column names in the SQL table match the property names in the <see cref="StswBindingList{TModel}"/>.
    /// </remarks>
    public static void Set<TModel>(this SqlTransaction sqlTran, StswBindingList<TModel> items, string tableName, IEnumerable<string>? setColumns = null, IEnumerable<string>? idColumns = null, int? timeout = null) where TModel : IStswCollectionItem, new()
        => sqlTran.Connection.Set(items, tableName, setColumns, idColumns, timeout, sqlTran);

    /// <summary>
    /// Performs insert, update, and delete operations on a SQL table based on the state of the items in the provided <see cref="StswBindingList{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the items in the list.</typeparam>
    /// <param name="items">The list of items to insert, update, or delete.</param>
    /// <param name="tableName">The name of the SQL table to modify.</param>
    /// <param name="idColumns">The columns used as identifiers in the table.</param>
    /// <param name="setColumns">The columns to be updated in the table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <remarks>
    /// This method assumes that the column names in the SQL table match the property names in the <see cref="StswBindingList{TModel}"/>.
    /// </remarks>
    public static void Set<TModel>(this StswDatabaseModel model, StswBindingList<TModel> items, string tableName, IEnumerable<string>? setColumns = null, IEnumerable<string>? idColumns = null, int? timeout = null, SqlTransaction? sqlTran = null) where TModel : IStswCollectionItem, new()
        => model.OpenedConnection().Set(items, tableName, setColumns, idColumns, timeout, sqlTran);

    /// <summary>
    /// Checks if the query can be executed based on the current application state.
    /// </summary>
    /// <returns>
    /// Returns <see langword="false"/> if the application is in design mode and queries should not be executed, otherwise returns <see langword="true"/>.
    /// </returns>
    public static bool CheckQueryConditions()
    {
        var isInDesignMode = false;
        if (StswDatabases.Config.ReturnIfInDesignerMode)
            Application.Current.Dispatcher.Invoke(() => isInDesignMode = DesignerProperties.GetIsInDesignMode(new()));
        if (isInDesignMode)
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
    /// Reduces the amount of space in the given SQL query string by removing unnecessary whitespace 
    /// while preserving the content within string literals.
    /// </summary>
    /// <param name="query">The SQL query to process and reduce unnecessary whitespace.</param>
    /// <returns>The processed SQL query with reduced whitespace.</returns>
    public static string LessSpaceQuery(string query)
    {
        var regex = new Regex(@"('([^']*)')|([^']+)");
        var parts = regex.Matches(query)
                         .Cast<Match>()
                         .Select(x => x.Groups[2].Success ? (x.Groups[2].Value, true) : (x.Groups[3].Value, false))
                         .ToList();

        return parts.Aggregate(string.Empty, (current, part) => current + (part.Item2 ? $"'{part.Value}'" : StswFn.RemoveConsecutiveText(part.Value.Replace("\t", " "), " ")));
    }

    /// <summary>
    /// Prepares the specified SQL command by clearing existing parameters and adding new ones based on 
    /// the provided model. The model can be an IEnumerable of SQL parameters, a dictionary, or an object 
    /// whose properties will be used as parameters.
    /// </summary>
    /// <param name="sqlCommand">The SQL command to prepare with parameters.</param>
    /// <param name="model">The model containing the values to be added as parameters.</param>
    /// <returns>The prepared <see cref="SqlCommand"/>.</returns>
    private static SqlCommand PrepareCommand(this SqlCommand sqlCommand, object? model)
    {
        sqlCommand.Parameters.Clear();

        if (model != null)
        {
            var sqlParameters = model switch
            {
                IEnumerable<SqlParameter> paramList => paramList.ToList(),
                IDictionary<string, object> dictionary => dictionary.Select(x => new SqlParameter("@" + x.Key, x.Value ?? DBNull.Value)).ToList(),
                _ => model.GetType().GetProperties().Select(x => new SqlParameter("@" + x.Name, x.GetValue(model) ?? DBNull.Value)).ToList()
            };
            
            foreach (var parameter in sqlParameters)
            {
                if (parameter.Value?.GetType()?.IsListType(out var type) == true)
                {
                    if (type == typeof(byte))
                        sqlCommand.Parameters.Add(parameter);
                    else if (type?.IsValueType == true)
                        sqlCommand.ParametersAddList(parameter.ParameterName, (IList?)parameter.Value);
                }
                else
                {
                    sqlCommand.Parameters.Add(parameter);
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
}
