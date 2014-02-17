using System;
using System.Data;

namespace Griffin.Data
{
    /// <summary>
    ///     An unit of work implementation which uses the interfaces that is defined in ADO.NET and should therefore work with
    ///     most ADO.NET driver implementations.
    /// </summary>
    /// <example>
    ///     <code>
    /// public IUnitOfWork Create()
    /// {
    ///     var conString = ConfigurationManager.ConnectionStrings("MyDb").ConnectionString;
    ///     var con = new SqlConnection(conString);
    ///     con.Open();
    ///     return new AdoNetUnitOfWork(con, true);
    /// }
    /// 
    /// UnitOfWorkFactory.SetFactoryMethod(() => Create());
    /// </code>
    /// </example>
    public class AdoNetUnitOfWork : IAdoNetUnitOfWork
    {
        private readonly bool _ownsConnection;
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        /// <summary>
        /// </summary>
        /// <param name="connection">
        ///     A connection may not be shared between multiple transactions, so make sure that the connection
        ///     is unique for this uow
        /// </param>
        public AdoNetUnitOfWork(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (connection.State != ConnectionState.Open)
                throw new DataException("Connection '" + connection.ConnectionString + "' is not open.");

            _connection = connection;
            _ownsConnection = false;
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// </summary>
        /// <param name="connection">
        ///     A connection may not be shared between multiple transactions, so make sure that the connection
        ///     is unique for this uow
        /// </param>
        /// <param name="ownsConnection">This unit of work owns the connection and will close it when being disposed.</param>
        public AdoNetUnitOfWork(IDbConnection connection, bool ownsConnection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
            _ownsConnection = ownsConnection;
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// </summary>
        /// <param name="connection">
        ///     A connection may not be shared between multiple transactions, so make sure that the connection
        ///     is unique for this uow
        /// </param>
        /// <param name="ownsConnection">This unit of work owns the connection and will close it when being disposed.</param>
        /// <param name="isolationLevel">Isolation level that the transaction should use.</param>
        public AdoNetUnitOfWork(IDbConnection connection, bool ownsConnection, IsolationLevel isolationLevel)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
            _ownsConnection = ownsConnection;
            _transaction = _connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }

            if (_ownsConnection && _connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        /// <summary>
        ///     Commit changes.
        /// </summary>
        /// <exception cref="TransactionAlreadyClosedException">Transaction have already been commited or disposed.</exception>
        public void SaveChanges()
        {
            if (_transaction == null)
                throw new TransactionAlreadyClosedException();

            _transaction.Commit();
            _transaction = null;
        }

        /// <summary>
        ///     Create a new command
        /// </summary>
        /// <returns>Created command (never <c>null</c>)</returns>
        /// <remarks>
        ///     <para>The created command have been enlisted in the local transaction which is wrapped by this Unit Of Work.</para>
        /// </remarks>
        /// <exception cref="DataException">Failed to create the command</exception>
        public IDbCommand CreateCommand()
        {
            var cmd = _connection.CreateCommand();
            cmd.Transaction = _transaction;
            return cmd;
        }

        /// <summary>
        ///     Execute a SQL query within the transaction
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void Execute(string sql, object parameters)
        {
            using (var cmd = CreateCommand())
            {
                cmd.CommandText = sql;
                var dictionary = parameters.ToDictionary();
                foreach (var kvp in dictionary)
                {
                    cmd.AddParameter(kvp.Key, kvp.Value);
                }
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Execute a SQL query within the transaction
        /// </summary>
        /// <param name="sql"></param>
        public void Execute(string sql)
        {
            using (var cmd = CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}