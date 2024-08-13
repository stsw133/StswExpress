using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.ComponentModel;

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
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    public static void BulkInsert<TModel>(this SqlConnection sqlConn, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
    {
        if (!CheckQueryConditions())
            return;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true);

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
    public static void BulkInsert<TModel>(this SqlTransaction sqlTran, IEnumerable<TModel> items, string tableName, int? timeout = null)
        => sqlTran.Connection.BulkInsert(items, tableName, timeout, sqlTran);

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="TModel">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="tableName">The name of the database table.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    public static void BulkInsert<TModel>(this StswDatabaseModel model, IEnumerable<TModel> items, string tableName, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().BulkInsert(items, tableName, timeout, sqlTran);

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteNonQuery(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
    {
        if (!CheckQueryConditions())
            return default;

        var models = parameters switch
        {
            IEnumerable<SqlParameter> => [parameters],
            IEnumerable<object?> enumerable => enumerable,
            _ => [parameters],
        };

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, models.Count() > 1);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;

        var result = 0;

        foreach (var modelParameters in models)
        {
            PrepareParameters(sqlCmd, modelParameters);
            result += sqlCmd.ExecuteNonQuery();
        }

        factory.Commit();
        return result;
    }

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteNonQuery(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteNonQuery(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteNonQuery(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteNonQuery(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    public static SqlDataReader? ExecuteReader(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
    {
        if (!CheckQueryConditions())
            return default;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);

        return factory.Transaction != null ? sqlCmd.ExecuteReader() : sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    public static SqlDataReader? ExecuteReader(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteReader(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    public static SqlDataReader? ExecuteReader(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteReader(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes the query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The scalar value.</returns>
    public static TResult? ExecuteScalar<TResult>(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
    {
        if (!CheckQueryConditions())
            return default;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false);
        using var sqlCmd = new SqlCommand(PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        var result = sqlCmd.ExecuteScalar().ConvertTo<TResult?>();
        factory.Commit();
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
    public static TResult? ExecuteScalar<TResult>(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteScalar<TResult>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes the query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The scalar value.</returns>
    public static TResult? ExecuteScalar<TResult>(this StswDatabaseModel model, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteScalar<TResult>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="procName">The name of the stored procedure.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteStoredProcedure(this SqlConnection sqlConn, string procName, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
    {
        if (!CheckQueryConditions())
            return default;

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, true);
        using var sqlCmd = new SqlCommand(procName, factory.Connection, factory.Transaction)
        {
            CommandType = CommandType.StoredProcedure,
        };
        sqlCmd.CommandTimeout = timeout ?? sqlCmd.CommandTimeout;

        PrepareParameters(sqlCmd, parameters);

        var result = sqlCmd.ExecuteNonQuery();

        factory.Commit();
        return result;
    }

    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="procName">The name of the stored procedure.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteStoredProcedure(this SqlTransaction sqlTran, string procName, object? parameters = null, int? timeout = null)
        => sqlTran.Connection.ExecuteStoredProcedure(procName, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="procName">The name of the stored procedure.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteStoredProcedure(this StswDatabaseModel model, string procName, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null)
        => model.OpenedConnection().ExecuteStoredProcedure(procName, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A collection of results.</returns>
    public static IEnumerable<TResult> Get<TResult>(this SqlConnection sqlConn, string query, object? parameters = null, int? timeout = null, SqlTransaction? sqlTran = null) where TResult : class, new()
    {
        if (!CheckQueryConditions())
            return [];

        using var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, false);
        using var sqlDA = new SqlDataAdapter(PrepareQuery(query), factory.Connection);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? sqlDA.SelectCommand.CommandTimeout;
        sqlDA.SelectCommand.Transaction = factory.Transaction;

        PrepareParameters(sqlDA.SelectCommand, parameters);

        var dataTable = new DataTable();
        sqlDA.Fill(dataTable);

        return dataTable.MapTo<TResult>(StswDatabases.DelimiterForMapping);
    }

    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A collection of results.</returns>
    public static IEnumerable<TResult> Get<TResult>(this SqlTransaction sqlTran, string query, object? parameters = null, int? timeout = null) where TResult : class, new()
        => sqlTran.Connection.Get<TResult>(query, parameters, timeout, sqlTran);

    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A collection of results.</returns>
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
        if (!CheckQueryConditions())
            return;

        /// prepare parameters
        inclusionProps ??= [];
        sqlParameters ??= [];

        var objProps = inclusionMode == StswInclusionMode.Include
            ? typeof(TModel).GetProperties().Where(x => x.Name.In(inclusionProps))
            : typeof(TModel).GetProperties().Where(x => !x.Name.In(inclusionProps.Union(input.IgnoredProperties)));
        var objPropsWithoutID = objProps.Where(x => x.Name != idProp);

        /// func
        using var sqlFactory = new StswSqlConnectionFactory(model.OpenedConnection(), null, true);

        var insertQuery = $"insert into {tableName} ({string.Join(", ", objPropsWithoutID.Select(x => x.Name))}) values ({string.Join(", ", objPropsWithoutID.Select(x => "@" + x.Name))})";
        foreach (var item in input.GetItemsByState(StswItemState.Added))
            using (var sqlCmd = new SqlCommand(insertQuery, sqlFactory.Connection, sqlFactory.Transaction))
            {
                sqlCmd.Parameters.AddRange([.. sqlParameters]);
                PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                sqlCmd.ExecuteNonQuery();
            }

        var updateQuery = $"update {tableName} set {string.Join(", ", objPropsWithoutID.Select(x => $"{x.Name}=@{x.Name}"))} where {idProp}=@{idProp}";
        foreach (var item in input.GetItemsByState(StswItemState.Modified))
            using (var sqlCmd = new SqlCommand(updateQuery, sqlFactory.Connection, sqlFactory.Transaction))
            {
                sqlCmd.Parameters.AddRange([.. sqlParameters]);
                PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                sqlCmd.ExecuteNonQuery();
            }

        var deleteQuery = $"delete from {tableName} where {idProp}=@{idProp}";
        foreach (var item in input.GetItemsByState(StswItemState.Deleted))
            using (var sqlCmd = new SqlCommand(deleteQuery, sqlFactory.Connection, sqlFactory.Transaction))
            {
                sqlCmd.Parameters.AddWithValue("@" + idProp, objProps.First(x => x.Name == idProp).GetValue(item));
                sqlCmd.ExecuteNonQuery();
            }

        sqlFactory.Commit();
    }

    /// <summary>
    /// Checks if the query can be executed based on the current application state. 
    /// This method verifies if the application is running in design mode and returns a boolean value indicating 
    /// whether the query should proceed or not.
    /// </summary>
    /// <returns>
    /// Returns <see langword="false"/> if the application is in design mode and queries should not be executed, otherwise returns <see langword="true"/>.
    /// </returns>
    public static bool CheckQueryConditions()
    {
        var isInDesignMode = false;
        if (StswDatabases.ReturnIfInDesignerMode)
            Application.Current.Dispatcher.Invoke(() => isInDesignMode = DesignerProperties.GetIsInDesignMode(new()));
        if (isInDesignMode)
            return false;

        return true;
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
    public static void PrepareParameters(SqlCommand sqlCommand, object? model)
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
                if (parameter.Value.GetType().IsListType(out var type))
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
    }

    /// <summary>
    /// Prepares the specified SQL command by adding SQL parameters and properties from the given item.
    /// This method is marked as obsolete due to the obsolescence of the associated Set method.
    /// </summary>
    /// <typeparam name="TModel">The type of the item containing the properties.</typeparam>
    /// <param name="sqlCommand">The SQL command to prepare with parameters and properties.</param>
    /// <param name="sqlParameters">The SQL parameters to add to the command.</param>
    /// <param name="propertyInfos">The properties of the item to add as parameters.</param>
    /// <param name="item">The item containing the properties to add as parameters.</param>
    [Obsolete($"This method is obsolete because {nameof(Set)} method is obsolete.")]
    public static void PrepareParameters<TModel>(SqlCommand sqlCommand, IEnumerable<SqlParameter>? sqlParameters, IEnumerable<PropertyInfo>? propertyInfos, TModel? item)
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
    private static string PrepareQuery(string query) => StswDatabases.MakeLessSpaceQuery ? LessSpaceQuery(query) : query;
}
