using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public static class StswDatabaseHelper
{
    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    public static void BulkInsert<TModel>(this SqlConnection connection, IEnumerable<TModel> items, string tableName, int? timeout = null)
    {
        using var sqlConn = connection;
        if (sqlConn.State != ConnectionState.Open)
            sqlConn.Open();
        using var bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.Default, null);
        bulkCopy.BulkCopyTimeout = timeout ?? bulkCopy.BulkCopyTimeout;
        bulkCopy.DestinationTableName = tableName;
        var dataTable = items.ToDataTable();
        bulkCopy.WriteToServer(dataTable);
    }

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    public static void BulkInsert<TModel>(this StswDatabaseModel model, IEnumerable<TModel> items, string tableName, int? timeout = null)
    {
        if (!model.CheckQueryConditions())
            return;

        if (model.Transaction != null)
            model.BulkInsertTransacted(items, tableName, timeout);
        else
            model.BulkInsertUntransacted(items, tableName, timeout);
    }

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    internal static void BulkInsertTransacted<TModel>(this StswDatabaseModel model, IEnumerable<TModel> items, string tableName, int? timeout = null)
    {
        using var bulkCopy = new SqlBulkCopy(model.Transaction!.Connection, SqlBulkCopyOptions.Default, model.Transaction);
        bulkCopy.BulkCopyTimeout = timeout ?? model.DefaultTimeout ?? bulkCopy.BulkCopyTimeout;
        bulkCopy.DestinationTableName = tableName;
        var dataTable = items.ToDataTable();
        bulkCopy.WriteToServer(dataTable);
    }

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    internal static void BulkInsertUntransacted<TModel>(this StswDatabaseModel model, IEnumerable<TModel> items, string tableName, int? timeout = null)
    {
        using var sqlConn = model.OpenedConnection();
        using var bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.Default, null);
        bulkCopy.BulkCopyTimeout = timeout ?? model.DefaultTimeout ?? bulkCopy.BulkCopyTimeout;
        bulkCopy.DestinationTableName = tableName;
        var dataTable = items.ToDataTable();
        bulkCopy.WriteToServer(dataTable);
    }

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteNonQuery(this SqlConnection connection, string query, object? parameters = null, int? timeout = null)
    {
        var models = parameters switch
        {
            IEnumerable<SqlParameter> => [parameters],
            IEnumerable<object?> enumerable => enumerable,
            _ => [parameters],
        };

        var result = 0;

        using var sqlConn = connection;
        if (sqlConn.State != ConnectionState.Open)
            sqlConn.Open();
        using var sqlTran = models?.Count() > 1 ? sqlConn.BeginTransaction() : null;
        using var sqlCmd = new SqlCommand(LessSpaceQuery(query), sqlConn, sqlTran);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        foreach (var model in models!)
        {
            PrepareParameters(sqlCmd, model);
            result += sqlCmd.ExecuteNonQuery();
        }
        sqlTran?.Commit();

        return result;
    }

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteNonQuery(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null)
    {
        if (!model.CheckQueryConditions())
            return default;

        var models = parameters switch
        {
            IEnumerable<SqlParameter> => [parameters],
            IEnumerable<object?> enumerable => enumerable,
            _ => [parameters],
        };

        if (model.Transaction != null)
            return model.ExecuteNonQueryTransacted(query, models, timeout);
        else
            return model.ExecuteNonQueryUntransacted(query, models, timeout);
    }
    
    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    internal static int ExecuteNonQueryTransacted(this StswDatabaseModel model, string query, IEnumerable<object?> parameters, int? timeout = null)
    {
        var result = 0;

        using var sqlCmd = new SqlCommand(model.PrepareQuery(query), model.Transaction!.Connection, model.Transaction);
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        foreach (var parameter in parameters)
        {
            PrepareParameters(sqlCmd, parameter);
            result += sqlCmd.ExecuteNonQuery();
        }

        return result;
    }

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    internal static int ExecuteNonQueryUntransacted(this StswDatabaseModel model, string query, IEnumerable<object?> parameters, int? timeout = null)
    {
        var result = 0;

        using var sqlConn = model.OpenedConnection();
        using var sqlTran = parameters.Count() > 1 ? sqlConn.BeginTransaction() : null;
        using var sqlCmd = new SqlCommand(model.PrepareQuery(query), sqlConn, sqlTran);
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        foreach (var parameter in parameters)
        {
            PrepareParameters(sqlCmd, parameter);
            result += sqlCmd.ExecuteNonQuery();
        }
        sqlTran?.Commit();

        return result;
    }

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    public static SqlDataReader? ExecuteReader(this SqlConnection connection, string query, object? parameters = null, int? timeout = null)
    {
        using var sqlConn = connection;
        if (sqlConn.State != ConnectionState.Open)
            sqlConn.Open();
        using var sqlCmd = new SqlCommand(LessSpaceQuery(query), sqlConn);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    public static SqlDataReader? ExecuteReader(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null)
    {
        if (!model.CheckQueryConditions())
            return default;

        if (model.Transaction != null)
            return model.ExecuteReaderTransacted(query, parameters, timeout);
        else
            return model.ExecuteReaderUntransacted(query, parameters, timeout);
    }

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    internal static SqlDataReader? ExecuteReaderTransacted(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null)
    {
        using var sqlCmd = new SqlCommand(model.PrepareQuery(query), model.Transaction!.Connection, model.Transaction);
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteReader();
    }

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    internal static SqlDataReader? ExecuteReaderUntransacted(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null)
    {
        using var sqlConn = model.OpenedConnection();
        using var sqlCmd = new SqlCommand(model.PrepareQuery(query), sqlConn);
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
    }
    
    /// <summary>
    /// Executes the query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The scalar value.</returns>
    public static TResult? ExecuteScalar<TResult>(this SqlConnection connection, string query, object? parameters = null, int? timeout = null)
    {
        using var sqlConn = connection;
        if (sqlConn.State != ConnectionState.Open)
            sqlConn.Open();
        using var sqlCmd = new SqlCommand(LessSpaceQuery(query), sqlConn);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteScalar().ConvertTo<TResult?>();
    }

    /// <summary>
    /// Executes the query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The scalar value.</returns>
    public static TResult? ExecuteScalar<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null)
    {
        if (!model.CheckQueryConditions())
            return default;

        if (model.Transaction != null)
            return model.ExecuteScalarTransacted<TResult?>(query, parameters, timeout);
        else
            return model.ExecuteScalarUntransacted<TResult?>(query, parameters, timeout);
    }

    /// <summary>
    /// Executes the query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The scalar value.</returns>
    internal static TResult? ExecuteScalarTransacted<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null)
    {
        using var sqlCmd = new SqlCommand(model.PrepareQuery(query), model.Transaction!.Connection, model.Transaction);
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteScalar().ConvertTo<TResult?>();
    }

    /// <summary>
    /// Executes the query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The scalar value.</returns>
    internal static TResult? ExecuteScalarUntransacted<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null)
    {
        using var sqlConn = model.OpenedConnection();
        using var sqlCmd = new SqlCommand(model.PrepareQuery(query), sqlConn);
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteScalar().ConvertTo<TResult?>();
    }

    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="procName">The name of the stored procedure.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteStoredProcedure(this SqlConnection connection, string procName, object? parameters = null, int? timeout = null)
    {
        using var sqlConn = connection;
        if (sqlConn.State != ConnectionState.Open)
            sqlConn.Open();
        using var sqlCmd = new SqlCommand(procName, sqlConn) { CommandType = CommandType.StoredProcedure };
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteNonQuery();
    }
    
    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="procName">The name of the stored procedure.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteStoredProcedure(this StswDatabaseModel model, string procName, object? parameters = null, int? timeout = null)
    {
        if (!model.CheckQueryConditions())
            return default;

        if (model.Transaction != null)
            return model.ExecuteStoredProcedureTransacted(procName, parameters, timeout);
        else
            return model.ExecuteStoredProcedureUntransacted(procName, parameters, timeout);
    }

    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="procName">The name of the stored procedure.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    internal static int? ExecuteStoredProcedureTransacted(this StswDatabaseModel model, string procName, object? parameters = null, int? timeout = null)
    {
        using var sqlCmd = new SqlCommand(procName, model.Transaction!.Connection, model.Transaction) { CommandType = CommandType.StoredProcedure };
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="procName">The name of the stored procedure.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    internal static int? ExecuteStoredProcedureUntransacted(this StswDatabaseModel model, string procName, object? parameters = null, int? timeout = null)
    {
        using var sqlConn = model.OpenedConnection();
        using var sqlCmd = new SqlCommand(procName, sqlConn) { CommandType = CommandType.StoredProcedure };
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        return sqlCmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A collection of results.</returns>
    public static IEnumerable<TResult> Get<TResult>(this SqlConnection connection, string query, object? parameters = null, int? timeout = null, char delimiter = '/') where TResult : class, new()
    {
        using var sqlConn = connection;
        if (sqlConn.State != ConnectionState.Open)
            sqlConn.Open();
        using var sqlDA = new SqlDataAdapter(LessSpaceQuery(query), sqlConn);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? sqlDA.SelectCommand.CommandTimeout;
        PrepareParameters(sqlDA.SelectCommand, parameters);

        var dt = new DataTable();
        sqlDA.Fill(dt);
        return dt.MapTo<TResult>(delimiter);
    }

    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A collection of results.</returns>
    public static IEnumerable<TResult> Get<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, char delimiter = '/') where TResult : class, new()
    {
        if (!model.CheckQueryConditions())
            return [];

        if (model.Transaction != null)
            return model.GetTransacted<TResult>(query, parameters, timeout).MapTo<TResult>(delimiter);
        else
            return model.GetUntransacted<TResult>(query, parameters, timeout).MapTo<TResult>(delimiter);
    }

    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A collection of results.</returns>
    internal static DataTable GetTransacted<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null) where TResult : class, new()
    {
        using var sqlDA = new SqlDataAdapter(model.PrepareQuery(query), model.Transaction!.Connection);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlDA.SelectCommand.CommandTimeout;
        sqlDA.SelectCommand.Transaction = model.Transaction;
        PrepareParameters(sqlDA.SelectCommand, parameters);

        var dt = new DataTable();
        sqlDA.Fill(dt);
        return dt;
    }
    
    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A collection of results.</returns>
    internal static DataTable GetUntransacted<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null) where TResult : class, new()
    {
        using var sqlConn = model.OpenedConnection();
        using var sqlDA = new SqlDataAdapter(model.PrepareQuery(query), sqlConn);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlDA.SelectCommand.CommandTimeout;
        PrepareParameters(sqlDA.SelectCommand, parameters);

        var dt = new DataTable();
        sqlDA.Fill(dt);
        return dt;
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
    public static void Set<TModel>(this StswDatabaseModel model, StswBindingList<TModel> input, string tableName, string idProp, StswInclusionMode inclusionMode = StswInclusionMode.Include, IEnumerable<string>? inclusionProps = null, IList<SqlParameter>? sqlParameters = null) where TModel : IStswCollectionItem, new()
    {
        if (!model.CheckQueryConditions())
            return;

        /// prepare parameters
        inclusionProps ??= [];
        sqlParameters ??= [];

        var objProps = inclusionMode == StswInclusionMode.Include
            ? typeof(TModel).GetProperties().Where(x => x.Name.In(inclusionProps))
            : typeof(TModel).GetProperties().Where(x => !x.Name.In(inclusionProps.Union(input.IgnoredProperties)));
        var objPropsWithoutID = objProps.Where(x => x.Name != idProp);

        /// func
        using var sqlTran = model.Transaction ?? model.OpenedConnection().BeginTransaction();

        var insertQuery = $"insert into {tableName} ({string.Join(", ", objPropsWithoutID.Select(x => x.Name))}) values ({string.Join(", ", objPropsWithoutID.Select(x => "@" + x.Name))})";
        foreach (var item in input.GetItemsByState(StswItemState.Added))
            using (var sqlCmd = new SqlCommand(insertQuery, sqlTran.Connection, sqlTran))
            {
                sqlCmd.Parameters.AddRange([.. sqlParameters]);
                StswDatabaseHelper.PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                sqlCmd.ExecuteNonQuery();
            }

        var updateQuery = $"update {tableName} set {string.Join(", ", objPropsWithoutID.Select(x => $"{x.Name}=@{x.Name}"))} where {idProp}=@{idProp}";
        foreach (var item in input.GetItemsByState(StswItemState.Modified))
            using (var sqlCmd = new SqlCommand(updateQuery, sqlTran.Connection, sqlTran))
            {
                sqlCmd.Parameters.AddRange([.. sqlParameters]);
                StswDatabaseHelper.PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                sqlCmd.ExecuteNonQuery();
            }

        var deleteQuery = $"delete from {tableName} where {idProp}=@{idProp}";
        foreach (var item in input.GetItemsByState(StswItemState.Deleted))
            using (var sqlCmd = new SqlCommand(deleteQuery, sqlTran.Connection, sqlTran))
            {
                sqlCmd.Parameters.AddWithValue("@" + idProp, objProps.First(x => x.Name == idProp).GetValue(item));
                sqlCmd.ExecuteNonQuery();
            }

        /// commit only if one-time transaction
        if (model.Transaction == null)
            sqlTran?.Commit();
    }

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
                         .Select(x => x.Groups[2].Success ? (x.Groups[2].Value, true) : (x.Groups[3].Value, false))
                         .ToList();

        return parts.Aggregate(string.Empty, (current, part) => current + (part.Item2 ? $"'{part.Value}'" : StswFn.RemoveConsecutiveText(part.Value.Replace("\t", " "), " ")));
    }
    
    /// <summary>
    /// Prepares the SQL command with the specified parameters.
    /// </summary>
    /// <param name="sqlCommand">The SQL command to prepare.</param>
    /// <param name="model">The model used for the query parameters.</param>
    internal static void PrepareParameters(SqlCommand sqlCommand, object? model)
    {
        sqlCommand.Parameters.Clear();

        if (model != null)
        {
            var sqlParameters = model switch
            {
                IEnumerable<SqlParameter> paramList => paramList.ToList(),
                IDictionary<string, object> dictionary => dictionary.Select(kvp => new SqlParameter("@" + kvp.Key, kvp.Value ?? DBNull.Value)).ToList(),
                _ => model.GetType().GetProperties().Select(x => new SqlParameter("@" + x.Name, x.GetValue(model) ?? DBNull.Value)).ToList()
            };

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
    [Obsolete($"This method is obsolete because {nameof(Set)} method is obsolete.")]
    internal static void PrepareParameters<TModel>(SqlCommand sqlCommand, IEnumerable<SqlParameter>? sqlParameters, IEnumerable<PropertyInfo>? propertyInfos, TModel? item)
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
}
