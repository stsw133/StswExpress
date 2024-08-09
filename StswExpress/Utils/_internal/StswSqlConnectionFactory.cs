using System;
using System.Data.SqlClient;
using System.Data;

namespace StswExpress;

/// <summary>
/// Provides a factory for managing SQL connections and transactions, ensuring proper resource handling and
/// offering flexibility in the use of transactions.
/// </summary>
internal class StswSqlConnectionFactory : IDisposable
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;
    private readonly bool _useTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswSqlConnectionFactory"/> class, opening a connection and optionally starting a transaction.
    /// </summary>
    /// <param name="model">The database model containing connection and transaction information.</param>
    /// <param name="useTransaction">
    /// If set to <see langword="true"/>, a new transaction is started if none exists. If <see langword="false"/>, no transaction is used.
    /// </param>
    public StswSqlConnectionFactory(StswDatabaseModel model, bool useTransaction = true)
    {
        _useTransaction = useTransaction;

        if (model.Transaction != null)
        {
            _connection = model.Transaction.Connection;
            _transaction = model.Transaction;
        }
        else
        {
            _connection = model.OpenedConnection();
            if (_useTransaction)
                _transaction = _connection.BeginTransaction();
        }
    }

    /// <summary>
    /// Gets the SQL connection managed by this factory.
    /// </summary>
    public SqlConnection Connection => _connection;

    /// <summary>
    /// Gets the SQL transaction managed by this factory, or <c>null</c> if no transaction is used.
    /// </summary>
    public SqlTransaction? Transaction => _transaction;

    /// <summary>
    /// Commits the current transaction if one is in use.
    /// </summary>
    public void Commit()
    {
        if (_useTransaction)
            _transaction?.Commit();
    }

    /// <summary>
    /// Rolls back the current transaction if one is in use.
    /// </summary>
    public void Rollback()
    {
        if (_useTransaction)
            _transaction?.Rollback();
    }

    /// <summary>
    /// Disposes the SQL connection and transaction, ensuring that all resources are properly released.
    /// </summary>
    public void Dispose()
    {
        if (_useTransaction)
            _transaction?.Dispose();

        if (_connection.State == ConnectionState.Open)
            _connection.Close();
        _connection.Dispose();
    }
}
