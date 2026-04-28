using System.Data;
using System.Data.Common;
using Dapper;
using Dommel;

namespace TradeHub.DAL
{
    public class DatabaseContext : IAsyncDisposable
    {
        private readonly DbConnection _connection;
        private DbTransaction? _transaction;

        static DatabaseContext()
        {
            // 1. Dapper Core: Map từ DB lên Object (Chiều đọc)
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            // 2. Dommel: Map Property -> Column (Chiều ghi)
            DommelMapper.SetColumnNameResolver(new SnakeCaseResolver());
        }

        public DatabaseContext(DbConnection connection)
        {
            _connection = connection;
        }

        public DbConnection Connection => _connection;
        public DbTransaction? Transaction => _transaction;

        public bool HasActiveTransaction => _transaction != null;

        public async Task EnsureOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
        }

        #region Transaction Handling

        private async Task BeginTransactionAsync()
        {
            await EnsureOpenAsync();
            _transaction ??= await _connection.BeginTransactionAsync();
        }

        private async Task CommitAsync()
        {
            if (_transaction == null) return;

            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        private async Task RollbackAsync()
        {
            if (_transaction == null) return;

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public virtual async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
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

        public virtual async Task ExecuteInTransactionAsync(Func<Task> action)
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

    public class SnakeCaseResolver : IColumnNameResolver
    {
        public string ResolveColumnName(System.Reflection.PropertyInfo propertyInfo)
        {
            var text = propertyInfo.Name;
            return string.Concat(text.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}