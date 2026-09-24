using Microsoft.Data.SqlClient;
using System.Data;

namespace StswExpress.Commons;

/// <summary>
/// Provides a factory for managing SQL connections and transactions, ensuring proper resource handling and
/// offering flexibility in the use of transactions.
/// </summary>
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
        : this(sqlConn, sqlTran, disposeConnection)
    {
        if (_isExternalTransaction)
            return;

        if (Connection.State != ConnectionState.Open)
            Connection.Open();

        if (useTransaction)
            Transaction = Connection.BeginTransaction();
    }

    private StswSqlConnectionFactory(SqlConnection sqlConn, SqlTransaction? sqlTran, bool? disposeConnection)
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
            Connection = sqlConn;
        }
    }

    /// <summary>
    /// Asynchronously creates a factory, opening the connection with cancellation support and optionally starting a transaction.
    /// </summary>
    public static async Task<StswSqlConnectionFactory> CreateAsync(
        SqlConnection sqlConn,
        SqlTransaction? sqlTran = null,
        bool useTransaction = true,
        bool? disposeConnection = null,
        CancellationToken cancellationToken = default)
    {
        var factory = new StswSqlConnectionFactory(sqlConn, sqlTran, disposeConnection);

        if (factory._isExternalTransaction)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return factory;
        }

        try
        {
            if (factory.Connection.State != ConnectionState.Open)
                await factory.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            if (useTransaction)
                factory.Transaction = factory.Connection.BeginTransaction();

            return factory;
        }
        catch
        {
            factory.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Commits the current transaction if one is in use.
    /// </summary>
    public void Commit()
    {
        if (!_isExternalTransaction && Transaction != null)
            Transaction.Commit();
    }

    /// <summary>
    /// Rolls back the current transaction if one is in use.
    /// </summary>
    public void Rollback()
    {
        if (!_isExternalTransaction && Transaction != null)
            Transaction.Rollback();
    }

    /// <summary>
    /// Disposes resources owned by the factory. External transactions are never disposed by the factory.
    /// </summary>
    public void Dispose()
    {
        if (_isExternalTransaction)
            return;

        Transaction?.Dispose();

        if (_disposeConnection)
            Connection.Dispose();
    }
}
