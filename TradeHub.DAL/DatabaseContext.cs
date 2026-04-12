using System.Data;
using MySqlConnector;
using Dapper;
using SqlKata.Compilers;

namespace TradeHub.DAL
{
    public class DatabaseContext : IAsyncDisposable
    {
        private readonly MySqlConnection _connection;
        private MySqlTransaction? _transaction;
        private static readonly Compiler _compiler = new MySqlCompiler();

        static DatabaseContext()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public DatabaseContext(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public Compiler Compiler => _compiler;
        public MySqlConnection Connection => _connection;
        public MySqlTransaction? Transaction => _transaction;

        public bool HasActiveTransaction => _transaction?.Connection != null;

        public async Task EnsureOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
        }

        #region Transaction Handling

        public async Task BeginTransactionAsync()
        {
            await EnsureOpenAsync();
            _transaction ??= await _connection.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null) return;

            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null) return;

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            if (_transaction != null)
                return await action();

            try
            {
                await BeginTransactionAsync();

                var result = await action();

                await CommitAsync();
                return result;
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                await action();
                return true;
            });
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();

            await _connection.DisposeAsync();
        }
    }
}