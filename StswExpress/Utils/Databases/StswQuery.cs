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
    /// <typeparam name="T">The type of the scalar value to return.</typeparam>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    /// <param name="sqlConnection">The SQL connection to use.</param>
    /// <param name="sqlTransaction">The SQL transaction to use.</param>
    /// <returns>The scalar value.</returns>
    public T ExecuteScalar<T>(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null)
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default!;

        using (_sqlConnection)
        {
            using var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
            PrepareParameters(sqlCmd, sqlParameters);
            return sqlCmd.ExecuteScalar().ConvertTo<T>()!;
        }
    }

    /// <summary>
    /// Executes the query and returns a scalar value or default if the query fails.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value to return.</typeparam>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    /// <param name="sqlConnection">The SQL connection to use.</param>
    /// <param name="sqlTransaction">The SQL transaction to use.</param>
    /// <returns>The scalar value or default.</returns>
    public T? TryExecuteScalar<T>(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null)
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default;

        using (_sqlConnection)
        {
            using var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
            PrepareParameters(sqlCmd, sqlParameters);
            using var sqlDR = sqlCmd.ExecuteReader();
            return sqlDR.Read() ? sqlDR[0].ConvertTo<T>() : default;
        }
    }

    /// <summary>
    /// Executes the query and returns a <see cref="SqlDataReader"/> for advanced data handling.
    /// </summary>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    /// <param name="sqlConnection">The SQL connection to use.</param>
    /// <param name="sqlTransaction">The SQL transaction to use.</param>
    /// <returns>A <see cref="SqlDataReader"/>.</returns>
    public SqlDataReader? ExecuteReader(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null)
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default;

        var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
        PrepareParameters(sqlCmd, sqlParameters);
        return sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// Executes the query and returns the number of rows affected.
    /// </summary>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    /// <param name="sqlConnection">The SQL connection to use.</param>
    /// <param name="sqlTransaction">The SQL transaction to use.</param>
    /// <returns>The number of rows affected.</returns>
    public int? ExecuteNonQuery(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null)
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default;

        using (_sqlConnection)
        {
            using var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
            PrepareParameters(sqlCmd, sqlParameters);
            return sqlCmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Executes the query and returns a collection of results.
    /// </summary>
    /// <typeparam name="T">The type of the results.</typeparam>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    /// <param name="sqlConnection">The SQL connection to use.</param>
    /// <param name="sqlTransaction">The SQL transaction to use.</param>
    /// <returns>A collection of results.</returns>
    public IEnumerable<T> Get<T>(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null) where T : class, new()
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default!;

        using (_sqlConnection)
        {
            using var sqlDA = new SqlDataAdapter(Query, _sqlConnection);
            PrepareParameters(sqlDA.SelectCommand, sqlParameters);

            var dt = new DataTable();
            sqlDA.Fill(dt);
            return dt.MapTo<T>();
        }
    }

    /// <summary>
    /// Executes the query and updates the database with the specified input collection.
    /// </summary>
    /// <typeparam name="T">The type of the items in the input collection.</typeparam>
    /// <param name="input">The input collection.</param>
    /// <param name="idProp">The property name of the ID.</param>
    /// <param name="inclusionMode">The inclusion mode.</param>
    /// <param name="inclusionProps">The properties to include or exclude based on the inclusion mode.</param>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    /// <param name="sqlConnection">The SQL connection to use.</param>
    /// <param name="sqlTransaction">The SQL transaction to use.</param>
    public void Set<T>(StswBindingList<T> input, string idProp, StswInclusionMode inclusionMode = StswInclusionMode.Include, IEnumerable<string>? inclusionProps = null, IList<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null) where T : IStswCollectionItem, new()
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return;

        /// prepare parameters
        inclusionProps ??= [];
        sqlParameters ??= [];

        var objProps = inclusionMode == StswInclusionMode.Include
            ? typeof(T).GetProperties().Where(x => x.Name.In(inclusionProps))
            : typeof(T).GetProperties().Where(x => !x.Name.In(inclusionProps.Union(input.IgnoredProperties)));
        var objPropsWithoutID = objProps.Where(x => x.Name != idProp);

        /// func
        using (_sqlConnection)
        {
            using (var sqlTran = _sqlTransaction ?? _sqlConnection?.BeginTransaction())
            {
                /// insert
                var query = $"insert into {Query} ({string.Join(", ", objPropsWithoutID.Select(x => x.Name))}) values ({string.Join(", ", objPropsWithoutID.Select(x => "@" + x.Name))})";
                foreach (var item in input.GetItemsByState(StswItemState.Added))
                    using (var sqlCmd = new SqlCommand(query, _sqlConnection, sqlTran))
                    {
                        sqlCmd.Parameters.AddRange([.. sqlParameters]);
                        PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                        sqlCmd.ExecuteNonQuery();
                    }
                /// update
                query = $"update {Query} set {string.Join(", ", objPropsWithoutID.Select(x => $"{x.Name}=@{x.Name}"))} where {idProp}=@{idProp}";
                foreach (var item in input.GetItemsByState(StswItemState.Modified))
                    using (var sqlCmd = new SqlCommand(query, _sqlConnection, sqlTran))
                    {
                        sqlCmd.Parameters.AddRange([.. sqlParameters]);
                        PrepareParameters(sqlCmd, sqlParameters, objProps, item);
                        sqlCmd.ExecuteNonQuery();
                    }
                /// delete
                query = $"delete from {Query} where {idProp}=@{idProp}";
                foreach (var item in input.GetItemsByState(StswItemState.Deleted))
                    using (var sqlCmd = new SqlCommand(query, _sqlConnection, sqlTran))
                    {
                        sqlCmd.Parameters.AddWithValue($"@{idProp}", objProps.First(x => x.Name == idProp).GetValue(item));
                        sqlCmd.ExecuteNonQuery();
                    }

                if (_sqlTransaction == null)
                    sqlTran?.Commit();
            }
        }
    }

    /// <summary>
    /// Performs a bulk insert operation to improve performance when inserting large datasets.
    /// </summary>
    /// <typeparam name="T">The type of the items to insert.</typeparam>
    /// <param name="items">The collection of items to insert.</param>
    /// <param name="sqlConnection">The SQL connection to use.</param>
    /// <param name="sqlTransaction">The SQL transaction to use.</param>
    public void BulkInsert<T>(IEnumerable<T> items, object? sqlConnection = null, SqlTransaction? sqlTransaction = null)
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return;

        using (var bulkCopy = new SqlBulkCopy(_sqlConnection, SqlBulkCopyOptions.Default, _sqlTransaction))
        {
            bulkCopy.DestinationTableName = Query;
            var dataTable = items.ToDataTable();
            bulkCopy.WriteToServer(dataTable);
        }
    }

    /// <summary>
    /// Executes a stored procedure with parameters.
    /// </summary>
    /// <param name="sqlParameters">The SQL parameters to use.</param>
    /// <param name="sqlConnection">The SQL connection to use.</param>
    /// <param name="sqlTransaction">The SQL transaction to use.</param>
    /// <returns>The number of rows affected.</returns>
    public int? ExecuteStoredProcedure(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null)
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default;

        using (var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction) { CommandType = CommandType.StoredProcedure })
        {
            PrepareParameters(sqlCmd, sqlParameters);
            return sqlCmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Reduces the amount of space in the query by removing unnecessary whitespace.
    /// </summary>
    /// <param name="query">The SQL query to process.</param>
    /// <returns>The processed SQL query.</returns>
    public static string LessSpaceQuery(string query)
    {
        var regex = new Regex(@"('([^']*)')|([^']+)");
        var matches = regex.Matches(query);
        var parts = new List<(string text, bool isInApostrophes)>();

        foreach (Match match in matches)
        {
            if (match.Groups[2].Success)
                parts.Add((match.Groups[2].Value, true));
            else
                parts.Add((match.Groups[3].Value, false));
        }

        query = string.Empty;
        foreach (var (text, isInApostrophes) in parts)
        {
            if (!isInApostrophes)
                query += StswFn.RemoveConsecutiveText(text.Replace("\t", " "), " ");
            else
                query += $"'{text}'";
        }

        return query;
    }

    /// <summary>
    /// Prepares the SQL connection and transaction.
    /// </summary>
    /// <param name="sqlConnection">The SQL connection to use.</param>
    /// <param name="sqlTransaction">The SQL transaction to use.</param>
    /// <returns>true if the connection is successfully prepared; otherwise, false.</returns>
    protected bool PrepareConnection(object? sqlConnection, SqlTransaction? sqlTransaction)
    {
        var isInDesignMode = false;
        if (ReturnIfInDesignerMode)
            Application.Current.Dispatcher.Invoke(() => isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject()));
        if (isInDesignMode)
            return false;

        SqlConnection? sqlConn = null;
        switch (sqlConnection)
        {
            case StswDatabaseModel stswDatabase:
                if (stswDatabase.SqlTransaction != null)
                {
                    sqlTransaction ??= stswDatabase.SqlTransaction;
                    sqlConn = stswDatabase.SqlTransaction.Connection;
                }
                else
                    sqlConn = stswDatabase.OpenedConnection();
                break;
            case SqlConnection sqlConnection1:
                sqlConn = sqlConnection1;
                break;
            case string sqlConnection2:
                sqlConn = new SqlConnection(sqlConnection2);
                break;
            default:
                if (Database != null)
                    if (Database.SqlTransaction != null)
                    {
                        sqlTransaction ??= Database.SqlTransaction;
                        sqlConn = Database.SqlTransaction.Connection;
                    }
                    else
                        sqlConn = new SqlConnection(Database.GetConnString());
                /*
                else if (StswDatabases.Current != null)
                    if (StswDatabases.Current.SqlTransaction != null)
                    {
                        sqlTransaction ??= StswDatabases.Current.SqlTransaction;
                        sqlConn = StswDatabases.Current.SqlTransaction.Connection;
                    }
                    else
                        sqlConn = new SqlConnection(StswDatabases.Current.GetConnString());
                */
                break;
        }

        _sqlConnection = sqlConn;
        _sqlTransaction = sqlTransaction;
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
    /// <param name="sqlParameters">The SQL parameters to add to the command.</param>
    protected void PrepareParameters(SqlCommand sqlCommand, IEnumerable<SqlParameter>? sqlParameters)
    {
        if (sqlParameters != null)
            foreach (var parameter in sqlParameters)
            {
                if (parameter.Value.GetType().IsListType(out var type) && type?.IsValueType == true)
                    sqlCommand.ParametersAddList(parameter.ParameterName, (IList?)parameter.Value);
                else
                    sqlCommand.Parameters.Add(parameter);
            }
    }

    /// <summary>
    /// Prepares the SQL command with the specified parameters and properties.
    /// </summary>
    /// <typeparam name="T">The type of the item containing the properties.</typeparam>
    /// <param name="sqlCommand">The SQL command to prepare.</param>
    /// <param name="sqlParameters">The SQL parameters to add to the command.</param>
    /// <param name="propertyInfos">The properties to add as parameters.</param>
    /// <param name="item">The item containing the properties.</param>
    protected void PrepareParameters<T>(SqlCommand sqlCommand, IEnumerable<SqlParameter>? sqlParameters, IEnumerable<PropertyInfo>? propertyInfos, T? item)
    {
        /// add parameters
        PrepareParameters(sqlCommand, sqlParameters);

        /// add properties as parameters
        if (propertyInfos != null)
            foreach (var prop in propertyInfos)
            {
                if (!sqlCommand.Parameters.Contains($"@{prop.Name}"))
                {
                    var value = prop.GetValue(item);
                    if (value == null)
                    {
                        sqlCommand.Parameters.Add($"@{prop.Name}", prop.PropertyType.InferSqlDbType()!.Value).Value = DBNull.Value;
                    }
                    else if (prop.PropertyType.IsListType(out var type) && type?.IsValueType == true)
                    {
                        if (type == typeof(byte))
                            sqlCommand.Parameters.AddWithValue($"@{prop.Name}", (byte[])value);
                        else
                            sqlCommand.ParametersAddList($"@{prop.Name}", (IList?)value);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
                    }
                }
            }
    }
}
