using Microsoft.Data.SqlClient;

namespace StswExpress.Commons;

/// <summary>
/// Provides a factory for managing SQL connections and transactions, ensuring proper resource handling and
/// offering flexibility in the use of transactions.
/// </summary>
[StswInfo("0.9.0")]
internal class StswSqlConnectionFactory : IDisposable
{
    private readonly bool _isExternalTransaction;
    private readonly bool _disposeConnection;

    public SqlConnection Connection { get; }
    public SqlTransaction? Transaction { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswSqlConnectionFactory"/> class, opening a connection and optionally starting a transaction.
    /// </summary>
    /// <param name="sqlConn">The SQL connection to use.</param>
    /// <param name="sqlTran">Optional SQL transaction to use. If provided, the connection must be associated with this transaction.</param>
    /// <param name="useTransaction">Indicates whether to start a new transaction if one is not provided.</param>
    /// <param name="disposeConnection">Optional flag indicating whether to dispose the connection when the factory is disposed. Defaults to the global configuration setting.</param>
    public StswSqlConnectionFactory(SqlConnection sqlConn, SqlTransaction? sqlTran = null, bool useTransaction = true, bool? disposeConnection = null)
    {
        _disposeConnection = disposeConnection ?? StswDatabases.Config.AutoDisposeConnection;

        if (sqlTran != null)
        {
            Connection = sqlTran.Connection ?? throw new InvalidOperationException("External transaction is not associated with any connection.");
            Transaction = sqlTran;
            _isExternalTransaction = true;
        }
        else
        {
            Connection = sqlConn.GetOpened();
            if (useTransaction)
                Transaction = Connection.BeginTransaction();
        }
    }

    /// <summary>
    /// Commits the current transaction if one is in use.
    /// </summary>
    public void Commit()
    {
        if (!_isExternalTransaction && Transaction != null)
            Transaction?.Commit();
    }

    /// <summary>
    /// Rolls back the current transaction if one is in use.
    /// </summary>
    public void Rollback()
    {
        if (!_isExternalTransaction && Transaction != null)
            Transaction?.Rollback();
    }

    /// <summary>
    /// Disposes the SQL connection and transaction, ensuring that all resources are properly released.
    /// </summary>
    public void Dispose()
    {
        if (_disposeConnection && !_isExternalTransaction)
        {
            Transaction?.Dispose();
            Connection.Dispose();
        }
    }
}
