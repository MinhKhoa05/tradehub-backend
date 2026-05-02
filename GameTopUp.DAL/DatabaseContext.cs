using System.Data;
using System.Data.Common;
using Dapper;
using Dommel;

namespace GameTopUp.DAL
{
    /// <summary>
    /// DatabaseContext quản lý kết nối và giao dịch (Transaction) với cơ sở dữ liệu.
    /// Việc tập trung quản lý Transaction tại đây giúp đảm bảo tính toàn vẹn dữ liệu (ACID)
    /// khi thực hiện nhiều thao tác ghi đồng thời trong một UseCase.
    /// </summary>
    public class DatabaseContext : IAsyncDisposable, IDisposable
    {
        private readonly DbConnection _connection;
        private DbTransaction? _transaction;

        static DatabaseContext()
        {
            // Cấu hình Dapper để tự động khớp các cột Snake Case (db_column) với Property Pascal Case (DbColumn).
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            // Cấu hình Dommel để thực hiện chuyển đổi tương tự khi sinh câu lệnh SQL tự động.
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
            {
                await _connection.OpenAsync();
            }
        }

        #region Transaction Handling

        private async Task BeginTransactionAsync()
        {
            await EnsureOpenAsync();
            if (_transaction == null)
            {
                _transaction = await _connection.BeginTransactionAsync();
            }
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

        /// <summary>
        /// Thực thi một chuỗi các hành động bên trong một Transaction.
        /// Nếu có lỗi xảy ra, toàn bộ các thay đổi sẽ được Rollback để bảo vệ dữ liệu.
        /// </summary>
        public virtual async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            // Nếu đã có transaction đang chạy (lồng nhau), chỉ thực thi action mà không tạo mới transaction.
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

        public virtual async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                await action();
                return true;
            });
        }

        #endregion

        public virtual async ValueTask DisposeAsync()
        {
            if (_transaction != null) await _transaction.DisposeAsync();
            if (_connection != null) await _connection.DisposeAsync();
        }

        public virtual void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }

    /// <summary>
    /// Chuyển đổi tên Property (PascalCase) sang tên cột DB (snake_case).
    /// </summary>
    public class SnakeCaseResolver : IColumnNameResolver
    {
        public string ResolveColumnName(System.Reflection.PropertyInfo propertyInfo)
        {
            var text = propertyInfo.Name;
            var result = string.Concat(text.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
            return result;
        }
    }
}
