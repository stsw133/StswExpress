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
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>A <see cref="SqlDataReader"/> instance for reading the data, or null if the query conditions are not met.</returns>
    public static SqlDataReader ExecuteReader(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteReader(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
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
    public static IEnumerable<TResult?> Get<TResult>(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
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
    public static IEnumerable<TResult?> Get<TResult>(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.Get<TResult?>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>A collection of results, or an empty collection if the query conditions are not met.</returns>
    public static IEnumerable<TResult?> Get<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().Get<TResult?>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a collection of results, where each result in `TResult1` is associated with matching items in `TResult2`.
    /// </summary>
    /// <typeparam name="TResult1">The type of the results to return.</typeparam>
    /// <typeparam name="TResult2">The type of the associated results to map to `TResult1`.</typeparam>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="sharedProp">The shared property used for matching `TResult1` and `TResult2`.</param>
    /// <param name="divideTo">The property in `TResult1` where associated `TResult2` items are stored.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <param name="disposeConnection">Whether to dispose the connection after execution.</param>
    /// <returns>A collection of results with associated items, or an empty collection if the query conditions are not met.</returns>
    public static IEnumerable<TResult1> GetDivided<TResult1, TResult2>(this SqlConnection sqlConn, string query, string sharedProp, string divideTo, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null, bool? disposeConnection = null)
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

        var result1 = dataTable.MapTo<TResult1>(StswDatabases.Config.DelimiterForMapping).Distinct();
        var result2 = dataTable.MapTo<TResult2>(StswDatabases.Config.DelimiterForMapping);

        var sharedPropInfo1 = typeof(TResult1).GetProperty(sharedProp);
        var sharedPropInfo2 = typeof(TResult2).GetProperty(sharedProp);
        var divideToPropInfo = typeof(TResult1).GetProperty(divideTo);

        if (sharedPropInfo1 == null || sharedPropInfo2 == null || divideToPropInfo == null)
            throw new ArgumentException($"Invalid property names for {nameof(sharedProp)} or {nameof(divideTo)}.");

        foreach (var result in result1)
        {
            var sharedValue = sharedPropInfo1.GetValue(result)?.ToString();
            var associatedResults = result2.Where(x => sharedPropInfo2.GetValue(x)?.ToString() == sharedValue);

            divideToPropInfo.SetValue(result, associatedResults);
        }

        return result1!;
    }

    /// <summary>
    /// Executes a SQL query and returns a collection of results, where each result in `TResult1` is associated with matching items in `TResult2`.
    /// </summary>
    /// <typeparam name="TResult1">The type of the results to return.</typeparam>
    /// <typeparam name="TResult2">The type of the associated results to map to `TResult1`.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="sharedProp">The shared property used for matching `TResult1` and `TResult2`.</param>
    /// <param name="divideTo">The property in `TResult1` where associated `TResult2` items are stored.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>A collection of results with associated items, or an empty collection if the query conditions are not met.</returns>
    public static IEnumerable<TResult1> GetDivided<TResult1, TResult2>(this SqlTransaction sqlTran, string query, string sharedProp, string divideTo, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.GetDivided<TResult1, TResult2>(query, sharedProp, divideTo, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a SQL query and returns a collection of results, where each result in `TResult1` is associated with matching items in `TResult2`.
    /// </summary>
    /// <typeparam name="TResult1">The type of the results to return.</typeparam>
    /// <typeparam name="TResult2">The type of the associated results to map to `TResult1`.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="sharedProp">The shared property used for matching `TResult1` and `TResult2`.</param>
    /// <param name="divideTo">The property in `TResult1` where associated `TResult2` items are stored.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <param name="sqlTran">The SQL transaction to use.</param>
    /// <returns>A collection of results with associated items, or an empty collection if the query conditions are not met.</returns>
    public static IEnumerable<TResult1> GetDivided<TResult1, TResult2>(this StswDatabaseModel model, string query, string sharedProp, string divideTo, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().GetDivided<TResult1, TResult2>(query, sharedProp, divideTo, parameters, timeout, sqlTran);

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
        if (!StswDatabases.Config.IsEnabled)
            return false;

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
            var sqlParameters = (model switch
            {
                IEnumerable<SqlParameter> paramList => paramList,
                IDictionary<string, object> dict => dict.Where(x => x.Key.ToLower().In(cmdParameters))
                                                        .Select(x => new SqlParameter("@" + x.Key, x.Value ?? DBNull.Value)),
                _ => model.GetType()
                          .GetProperties()
                          .Where(x => x.Name.ToLower().In(cmdParameters))
                          .Select(x => new SqlParameter("@" + x.Name, x.GetValue(model) ?? DBNull.Value))
            }).ToList();
            
            /// add prepared parameters to command
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
