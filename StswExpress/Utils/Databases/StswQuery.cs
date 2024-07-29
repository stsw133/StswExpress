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
/// Represents a query to be executed against a SQL database.
/// </summary>
public class StswQuery
{
    /// <summary>
    /// Gets or sets a value indicating whether to always make less space in the query.
    /// </summary>
    public static bool AlwaysMakeLessSpaceQuery { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to always return if in designer mode.
    /// </summary>
    public static bool AlwaysReturnIfInDesignerMode { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to always return if no database is available.
    /// </summary>
    public static bool AlwaysReturnIfNoDatabase { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswQuery"/> class with the specified query and optional database model.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="stswDb">The database model to use. If null, the current database model will be used.</param>
    public StswQuery(string query, StswDatabaseModel? stswDb = null)
    {
        Database = stswDb ?? StswDatabases.Current;
        Query = MakeLessSpaceQuery ? LessSpaceQuery(query) : query;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to make less space in the query.
    /// </summary>
    public bool MakeLessSpaceQuery { get; set; } = AlwaysMakeLessSpaceQuery;

    /// <summary>
    /// Gets or sets a value indicating whether to return if in designer mode.
    /// </summary>
    public bool ReturnIfInDesignerMode { get; set; } = AlwaysReturnIfInDesignerMode;

    /// <summary>
    /// Gets or sets a value indicating whether to return if no database is available.
    /// </summary>
    public bool ReturnIfNoDatabase { get; set; } = AlwaysReturnIfNoDatabase;

    /// <summary>
    /// Gets or sets the database model.
    /// </summary>
    public StswDatabaseModel? Database { protected get; set; }
    
    /// <summary>
    /// Gets or sets the SQL query.
    /// </summary>
    public string Query { get; protected set; }

    /// <summary>
    /// Executes the query and returns a scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <typeparam name="TModel">The type of the model used for the query parameters.</typeparam>
    /// <param name="model">The model used for the query parameters.</param>
    /// <returns>The scalar value.</returns>
    public TResult ExecuteScalar<TResult, TModel>(TModel? model) where TModel : class
    {
        if (!PrepareConnection())
            return default!;

        using var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
        PrepareParameters(sqlCmd, model);
        return sqlCmd.ExecuteScalar().ConvertTo<TResult>()!;
    }
    public TResult ExecuteScalar<TResult>(IEnumerable<SqlParameter>? sqlParameters = null) => ExecuteScalar<TResult, IEnumerable<SqlParameter>>(sqlParameters);

    /// <summary>
    /// Executes the query and returns a scalar value or default if the query fails.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value to return.</typeparam>
    /// <typeparam name="TModel">The type of the model used for the query parameters.</typeparam>
    /// <param name="model">The model used for the query parameters.</param>
    /// <returns>The scalar value or default.</returns>
    public TResult? TryExecuteScalar<TResult, TModel>(TModel? model) where TModel : class
    {
        if (!PrepareConnection())
            return default;

        using var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
        PrepareParameters(sqlCmd, model);
        using var sqlDR = sqlCmd.ExecuteReader();
        return sqlDR.Read() ? sqlDR[0].ConvertTo<TResult>() : default;
    }
    public TResult? TryExecuteScalar<TResult>(IEnumerable<SqlParameter>? sqlParameters = null) => TryExecuteScalar<TResult, IEnumerable<SqlParameter>>(sqlParameters);

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <typeparam name="TModel">The type of the model used for the query parameters.</typeparam>
    /// <param name="model">The model used for the query parameters.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    public SqlDataReader? ExecuteReader<TResult>(TResult? model) where TResult : class
    {
        if (!PrepareConnection())
            return default;

        var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
        PrepareParameters(sqlCmd, model);
        return sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
    }
    public SqlDataReader? ExecuteReader(IEnumerable<SqlParameter>? sqlParameters = null) => ExecuteReader<IEnumerable<SqlParameter>>(sqlParameters);

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <typeparam name="T">The type of the model used for the query parameters.</typeparam>
    /// <param name="models">The models used for the query parameters.</param>
    /// <returns>The number of rows affected.</returns>
    public int? ExecuteNonQuery<T>(IEnumerable<T?>? models) where T : class
    {
        if (!PrepareConnection())
            return default;

        var result = 0;
        var sqlTran = _sqlTransaction ?? (models?.Count() > 1 ? _sqlConnection?.BeginTransaction() : null);
        using var sqlCmd = new SqlCommand(Query, _sqlConnection, sqlTran);
        foreach (var model in models ?? [null])
        {
            PrepareParameters(sqlCmd, model);
            result += sqlCmd.ExecuteNonQuery();
        }

        /// commit only if one-time transaction
        if (_sqlTransaction == null)
            sqlTran?.Commit();

        return result;
    }
    public int? ExecuteNonQuery<T>(T? model) where T : class => ExecuteNonQuery([model]);
    public int? ExecuteNonQuery(IEnumerable<SqlParameter>? sqlParameters = null) => ExecuteNonQuery<SqlParameter>(sqlParameters?.ToList());

    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <typeparam name="TModel">The type of the model used for the query parameters.</typeparam>
    /// <param name="model">The model used for the query parameters.</param>
    /// <returns>A collection of results.</returns>
    public IEnumerable<TResult> Get<TResult, TModel>(TModel? model) where TModel : class where TResult : class, new()
    {
        if (!PrepareConnection())
            return default!;

        using var sqlDA = new SqlDataAdapter(Query, _sqlConnection);
        PrepareParameters(sqlDA.SelectCommand, model);

        var dt = new DataTable();
        sqlDA.Fill(dt);
        return dt.MapTo<TResult>();
    }
    public IEnumerable<TResult> Get<TResult>(IEnumerable<SqlParameter>? sqlParameters = null) where TResult : class, new() => Get<TResult, IEnumerable<SqlParameter>>(sqlParameters);

    /// <summary>
    /// Executes the query and updates the database with the specified input collection.
    /// </summary>
    /// <typeparam name="TModel">The type of the items in the input collection.</typeparam>
    /// <param name="input">The input collection.</param>
    /// <param name="idProp">The property name of the ID.</param>
    /// <param name="inclusionMode">The inclusion mode.</param>
    /// <param name="inclusionProps">The properties to include or exclude based on the inclusion mode.</param>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    public void Set<TModel>(StswBindingList<TModel> input, string idProp, StswInclusionMode inclusionMode = StswInclusionMode.Include, IEnumerable<string>? inclusionProps = null, IList<SqlParameter>? sqlParameters = null) where TModel : IStswCollectionItem, new()
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
        
        var insertQuery = $"insert into {Query} ({string.Join(", ", objPropsWithoutID.Select(x => x.Name))}) values ({string.Join(", ", objPropsWithoutID.Select(x => "@" + x.Name))})";
        foreach (var item in input.GetItemsByState(StswItemState.Added))
            using (var sqlCmd = new SqlCommand(insertQuery, _sqlConnection, sqlTran))
            {
                sqlCmd.Parameters.AddRange([.. sqlParameters]);
                PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                sqlCmd.ExecuteNonQuery();
            }
        
        var updateQuery = $"update {Query} set {string.Join(", ", objPropsWithoutID.Select(x => $"{x.Name}=@{x.Name}"))} where {idProp}=@{idProp}";
        foreach (var item in input.GetItemsByState(StswItemState.Modified))
            using (var sqlCmd = new SqlCommand(updateQuery, _sqlConnection, sqlTran))
            {
                sqlCmd.Parameters.AddRange([.. sqlParameters]);
                PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                sqlCmd.ExecuteNonQuery();
            }
        
        var deleteQuery = $"delete from {Query} where {idProp}=@{idProp}";
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
    public void BulkInsert<TModel>(IEnumerable<TModel> items)
    {
        if (!PrepareConnection())
            return;

        using var bulkCopy = new SqlBulkCopy(_sqlConnection, SqlBulkCopyOptions.Default, _sqlTransaction);
        bulkCopy.DestinationTableName = Query;
        var dataTable = items.ToDataTable();
        bulkCopy.WriteToServer(dataTable);
    }

    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    /// <returns>The number of rows affected.</returns>
    public int? ExecuteStoredProcedure(IEnumerable<SqlParameter>? sqlParameters = null)
    {
        if (!PrepareConnection())
            return default;

        using var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction) { CommandType = CommandType.StoredProcedure };
        PrepareParameters(sqlCmd, sqlParameters);
        return sqlCmd.ExecuteNonQuery();
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

        if (Database?.SqlTransaction != null)
        {
            _sqlTransaction ??= Database.SqlTransaction;
            _sqlConnection = Database.SqlTransaction.Connection;
        }
        else
        {
            _sqlConnection = Database?.OpenedConnection();
        }

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
    protected void PrepareParameters(SqlCommand sqlCommand, object? model)
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
}
