using System.Data;
using Dapper;
using MySqlConnector;

namespace TradeHub.DAL
{
    public class DatabaseContext : IDisposable
    {
        private readonly IDbConnection _connection;
        private IDbTransaction? _transaction;

        public DatabaseContext(string connectionString)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        #region Transaction

        public void BeginTransaction()
            => _transaction ??= _connection.BeginTransaction();

        public void Commit()
        {
            _transaction?.Commit();
            _transaction = null;
        }

        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction = null;
        }

        public void ExecuteInTransaction(Action action)
        {
            try
            {
                BeginTransaction();
                action();
                Commit();
            }
            catch
            {
                Rollback();
                throw;
            }
        }

        #endregion

        #region Dapper Helpers

        public int Execute(string sql, object? param = null)
            => _connection.Execute(sql, param, _transaction);

        public T ExecuteScalar<T>(string sql, object? param = null)
            => _connection.ExecuteScalar<T>(sql, param, _transaction);

        public T? QuerySingle<T>(string sql, object? param = null)
            => _connection.QuerySingleOrDefault<T>(sql, param, _transaction);

        public List<T> QueryList<T>(string sql, object? param = null)
            => _connection.Query<T>(sql, param, _transaction).AsList();

        #endregion

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection.Dispose();
        }
    }
}