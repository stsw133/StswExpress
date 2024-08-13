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
/// A static helper class that provides extension methods for performing common database operations 
/// such as bulk inserts, executing queries, and handling stored procedures in a more efficient manner.
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
        => new StswDatabaseModel(connection).BulkInsert(items, tableName, timeout);

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

        using var factory = new StswSqlConnectionFactory(model, true);

        using var bulkCopy = new SqlBulkCopy(factory.Connection, SqlBulkCopyOptions.Default, factory.Transaction);
        bulkCopy.BulkCopyTimeout = timeout ?? model.DefaultTimeout ?? bulkCopy.BulkCopyTimeout;
        bulkCopy.DestinationTableName = tableName;

        var dataTable = items.ToDataTable();
        bulkCopy.WriteToServer(dataTable);

        factory.Commit();
    }

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The models used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int? ExecuteNonQuery(this SqlConnection connection, string query, object? parameters = null, int? timeout = null)
        => new StswDatabaseModel(connection).ExecuteNonQuery(query, parameters, timeout);

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

        using var factory = new StswSqlConnectionFactory(model, models.Count() > 1);
        using var sqlCmd = new SqlCommand(model.PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;

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
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">The model used for the query parameters.</param>
    /// <param name="timeout">The timeout used for the command.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    public static SqlDataReader? ExecuteReader(this SqlConnection connection, string query, object? parameters = null, int? timeout = null)
        => new StswDatabaseModel(connection).ExecuteReader(query, parameters, timeout);

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

        using var factory = new StswSqlConnectionFactory(model);
        using var sqlCmd = new SqlCommand(model.PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);

        return factory.Transaction != null ? sqlCmd.ExecuteReader() : sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
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
        => new StswDatabaseModel(connection).ExecuteScalar<TResult>(query, parameters, timeout);

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

        using var factory = new StswSqlConnectionFactory(model, false);
        using var sqlCmd = new SqlCommand(model.PrepareQuery(query), factory.Connection, factory.Transaction);
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;
        PrepareParameters(sqlCmd, parameters);
        var result = sqlCmd.ExecuteScalar().ConvertTo<TResult?>();
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
    public static int? ExecuteStoredProcedure(this SqlConnection connection, string procName, object? parameters = null, int? timeout = null)
        => new StswDatabaseModel(connection).ExecuteStoredProcedure(procName, parameters, timeout);

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

        using var factory = new StswSqlConnectionFactory(model);
        using var sqlCmd = new SqlCommand(procName, factory.Connection, factory.Transaction)
        {
            CommandType = CommandType.StoredProcedure,
        };
        sqlCmd.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlCmd.CommandTimeout;

        PrepareParameters(sqlCmd, parameters);

        var result = sqlCmd.ExecuteNonQuery();

        factory.Commit();
        return result;
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
        => new StswDatabaseModel(connection).Get<TResult>(query, parameters, timeout, delimiter);

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

        using var factory = new StswSqlConnectionFactory(model, false);
        using var sqlDA = new SqlDataAdapter(model.PrepareQuery(query), factory.Connection);
        sqlDA.SelectCommand.CommandTimeout = timeout ?? model.DefaultTimeout ?? sqlDA.SelectCommand.CommandTimeout;
        sqlDA.SelectCommand.Transaction = factory.Transaction;

        PrepareParameters(sqlDA.SelectCommand, parameters);

        var dataTable = new DataTable();
        sqlDA.Fill(dataTable);

        return dataTable.MapTo<TResult>(delimiter);
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
                PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                sqlCmd.ExecuteNonQuery();
            }

        var updateQuery = $"update {tableName} set {string.Join(", ", objPropsWithoutID.Select(x => $"{x.Name}=@{x.Name}"))} where {idProp}=@{idProp}";
        foreach (var item in input.GetItemsByState(StswItemState.Modified))
            using (var sqlCmd = new SqlCommand(updateQuery, sqlTran.Connection, sqlTran))
            {
                sqlCmd.Parameters.AddRange([.. sqlParameters]);
                PrepareParameters(sqlCmd, sqlParameters, objProps, item);
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
