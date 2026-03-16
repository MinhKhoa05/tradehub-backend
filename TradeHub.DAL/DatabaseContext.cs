using System.Data;
using Dapper;
using MySqlConnector;

namespace TradeHub.DAL
{
    public class DatabaseContext : IAsyncDisposable
    {

        private readonly MySqlConnection _connection;
        private MySqlTransaction? _transaction;

        static DatabaseContext()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public DatabaseContext(string connectionString)
        {
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

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            if (_transaction != null)
            {
                return await action();
            }

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

        #region Dapper Async Helpers

        public async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            await OpenAsync();
            return await _connection.ExecuteAsync(sql, param, _transaction);
        }

        public async Task<int> ExecuteInsertAsync(string sql, object? param = null)
        {
            await OpenAsync();
            return await _connection.ExecuteScalarAsync<int>(sql + "; SELECT LAST_INSERT_ID();", param, _transaction);
        }

        public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
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
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            await _connection.DisposeAsync();
        }
    }
}