using System.Data;
using Dapper;
using MySqlConnector;

namespace TradeHub.DAL
{
    public class DatabaseContext : IAsyncDisposable
    {
        private readonly MySqlConnection _connection;
        private MySqlTransaction? _transaction;

        public DatabaseContext(string connectionString)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            _connection = new MySqlConnection(connectionString);
        }

        #region Connection

        public async Task OpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
        }

        #endregion

        #region Transaction

        public async Task BeginTransactionAsync()
        {
            await OpenAsync();
            _transaction ??= await _connection.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            try
            {
                await BeginTransactionAsync();
                await action();
                await CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        #endregion

        #region Dapper Async Helpers

        public async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            await OpenAsync();
            return await _connection.ExecuteAsync(sql, param, _transaction);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null)
        {
            await OpenAsync();
            return await _connection.ExecuteScalarAsync<T>(sql, param, _transaction);
        }

        public async Task<T?> QuerySingleAsync<T>(string sql, object? param = null)
        {
            await OpenAsync();
            return await _connection.QuerySingleOrDefaultAsync<T>(sql, param, _transaction);
        }

        public async Task<List<T>> QueryListAsync<T>(string sql, object? param = null)
        {
            await OpenAsync();
            var result = await _connection.QueryAsync<T>(sql, param, _transaction);
            return result.AsList();
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