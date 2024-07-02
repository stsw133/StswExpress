using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StswExpress;
/// <summary>
/// 
/// </summary>
public class StswQuery(string query, StswDatabaseModel? stswDb = null)
{
    /// static properties
    public static bool AlwaysMakeLessSpaceQuery { get; set; } = true;
    public static bool AlwaysReturnIfInDesignerMode { get; set; } = true;
    public static bool AlwaysReturnIfNoDatabase { get; set; } = false;

    /// local properties
    //public bool MakeLessSpaceQuery { get; set; } = AlwaysMakeLessSpaceQuery;
    //public bool ReturnIfInDesignerMode { get; set; } = AlwaysReturnIfInDesignerMode;
    public bool ReturnIfNoDatabase { get; set; } = AlwaysReturnIfNoDatabase;

    public StswDatabaseModel? Database { protected get; set; } = stswDb ?? StswDatabases.Current;
    public string Query { get; protected set; } = AlwaysMakeLessSpaceQuery ? LessSpaceQuery(query) : query;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<T> Get<T>(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null) where T : class, new()
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default!;
        
        using (_sqlConnection)
        {
            _sqlConnection?.Open();

            using var sqlDA = new SqlDataAdapter(Query, _sqlConnection);
            sqlDA.SelectCommand.Parameters.AddRange([.. sqlParameters]);

            var dt = new DataTable();
            sqlDA.Fill(dt);
            return dt.MapTo<T>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void Set<T>(StswBindingList<T> input, string idProp, Dictionary<string, bool>? properties = null, IList<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null) where T : IStswCollectionItem, new()
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return;

        /// prepare parameters
        properties ??= [];
        sqlParameters ??= [];

        IEnumerable<PropertyInfo> objProps;
        if (properties.Any(x => x.Value))
            objProps = typeof(T).GetProperties().Where(x => x.Name.In(properties.Where(x => x.Value).Select(x => x.Key)));
        else
            objProps = typeof(T).GetProperties().Where(x => !x.Name.In(properties.Where(x => !x.Value).Select(x => x.Key).Union(input.IgnoredProperties)));

        var objPropsWithoutID = objProps.Where(x => x.Name != idProp);
        foreach (var prop in objProps)
            if (!sqlParameters.Any(x => x.ParameterName == $"@{prop.Name}"))
                sqlParameters.Add(new($"@{prop.Name}", prop.PropertyType.ToSqlDbType()));

        /// func
        using (_sqlConnection)
        {
            _sqlConnection?.Open();

            using (var sqlTran = _sqlTransaction ?? _sqlConnection?.BeginTransaction())
            {
                /// insert
                var query = $"insert into {Query} ({string.Join(", ", objPropsWithoutID.Select(x => x.Name))}) values ({string.Join(", ", objPropsWithoutID.Select(x => "@" + x.Name))})";
                foreach (var item in input.GetItemsByState(StswItemState.Added))
                    using (var sqlCmd = new SqlCommand(query, _sqlConnection, sqlTran))
                    {
                        sqlCmd.Parameters.AddRange([.. sqlParameters]);
                        foreach (SqlParameter param in sqlCmd.Parameters)
                            param.SqlValue = objPropsWithoutID.First(x => x.Name == param.ParameterName[1..]).GetValue(item) ?? DBNull.Value;
                        sqlCmd.ExecuteNonQuery();
                    }
                /// update
                query = $"update {Query} set {string.Join(", ", objPropsWithoutID.Select(x => $"{x.Name}=@{x.Name}"))} where {idProp}=@{idProp}";
                foreach (var item in input.GetItemsByState(StswItemState.Modified))
                    using (var sqlCmd = new SqlCommand(query, _sqlConnection, sqlTran))
                    {
                        sqlCmd.Parameters.AddRange([.. sqlParameters]);
                        foreach (SqlParameter param in sqlCmd.Parameters)
                            param.SqlValue = objProps.First(x => x.Name == param.ParameterName[1..]).GetValue(item) ?? DBNull.Value;
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

                if (sqlTransaction == null)
                    sqlTran?.Commit();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public T ExecuteScalar<T>(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null)
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default!;

        using (_sqlConnection)
        {
            using var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
            sqlCmd.Parameters.AddRange([.. sqlParameters]);
            return sqlCmd.ExecuteScalar().ConvertTo<T>()!;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public T? TryExecuteScalar<T>(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null)
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default;

        using (_sqlConnection)
        {
            using var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
            sqlCmd.Parameters.AddRange([.. sqlParameters]);
            using var sqlDR = sqlCmd.ExecuteReader();
            return sqlDR.Read() ? sqlDR[0].ConvertTo<T>() : default;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int? ExecuteNonQuery(IEnumerable<SqlParameter>? sqlParameters = null, object? sqlConnection = null, SqlTransaction? sqlTransaction = null)
    {
        if (!PrepareConnection(sqlConnection, sqlTransaction))
            return default;

        using (_sqlConnection)
        {
            using var sqlCmd = new SqlCommand(Query, _sqlConnection, _sqlTransaction);
            sqlCmd.Parameters.AddRange([.. sqlParameters]);
            return sqlCmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlConnection"></param>
    /// <param name="sqlTransaction"></param>
    /// <returns></returns>
    protected bool PrepareConnection(object? sqlConnection, SqlTransaction? sqlTransaction)
    {
        //if (ReturnIfInDesignerMode && DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
        //    return false;

        /// connection
        SqlConnection? sqlConn = null;
        switch (sqlConnection)
        {
            case StswDatabaseModel stswDatabase:
                if (stswDatabase.SqlTranaction != null)
                {
                    sqlTransaction ??= stswDatabase.SqlTranaction;
                    sqlConn = stswDatabase.SqlTranaction.Connection;
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
                    if (Database.SqlTranaction != null)
                    {
                        sqlTransaction ??= Database.SqlTranaction;
                        sqlConn = Database.SqlTranaction.Connection;
                    }
                    else
                        sqlConn = Database.OpenedConnection();
                else if (StswDatabases.Current != null)
                    if (StswDatabases.Current.SqlTranaction != null)
                    {
                        sqlTransaction ??= StswDatabases.Current.SqlTranaction;
                        sqlConn = StswDatabases.Current.SqlTranaction.Connection;
                    }
                    else
                        sqlConn = StswDatabases.Current.OpenedConnection();
                break;
        }

        _sqlConnection = sqlConn;
        _sqlTransaction = sqlTransaction;
        if (ReturnIfNoDatabase && _sqlConnection == null)
            return false;

        _sqlConnection?.Open();
        return true;
    }
    private SqlConnection? _sqlConnection;
    private SqlTransaction? _sqlTransaction;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static string LessSpaceQuery(string query)
    {
        var regex = new Regex(@"('([^']*)')|([^']+)");
        var matches = regex.Matches(query);
        List<(string text, bool isInApostrophes)> parts = [];

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
}
