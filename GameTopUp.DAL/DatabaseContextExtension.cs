using Dapper;
using Dommel;

namespace GameTopUp.DAL
{
    /// <summary>
    /// Dommel -> CRUD don gi?n
    /// Dapper -> query ph?c t?p / partial update
    /// </summary>
    public static class DatabaseExtensions
    {
        #region --- DOMMEL ---

        /// <summary>Get theo PK (simple only)</summary>
        public static async Task<T?> GetByIdAsync<T>(this DatabaseContext db, object id) where T : class
        {
            await db.EnsureOpenAsync();
            return await db.Connection.GetAsync<T>(id, db.Transaction);
        }

        /// <summary>Insert entity don gi?n</summary>
        public static async Task<TId?> InsertAsync<T, TId>(this DatabaseContext db, T entity) where T : class
        {
            await db.EnsureOpenAsync();
            var id = await db.Connection.InsertAsync(entity, db.Transaction);
            return id is null ? default : (TId)id;
        }

        /// <summary>Update to�n b? entity</summary>
        public static async Task<bool> UpdateAsync<T>(this DatabaseContext db, T entity) where T : class
        {
            await db.EnsureOpenAsync();
            return await db.Connection.UpdateAsync(entity, db.Transaction);
        }

        /// <summary>Delete don gi?n</summary>
        public static async Task<bool> DeleteAsync<T>(this DatabaseContext db, T entity) where T : class
        {
            await db.EnsureOpenAsync();
            return await db.Connection.DeleteAsync(entity, db.Transaction);
        }

        #endregion

        #region --- DAPPER ---

        /// <summary>Query list (JOIN, filter...)</summary>
        public static async Task<List<T>> QueryAsync<T>(this DatabaseContext db, string sql, object? param = null)
        {
            await db.EnsureOpenAsync();
            return (await db.Connection.QueryAsync<T>(sql, param, db.Transaction)).ToList();
        }

        /// <summary>Query 1 record</summary>
        public static async Task<T?> QueryFirstAsync<T>(this DatabaseContext db, string sql, object? param = null)
        {
            await db.EnsureOpenAsync();
            return await db.Connection.QueryFirstOrDefaultAsync<T>(sql, param, db.Transaction);
        }

        /// <summary>Execute (INSERT/UPDATE/DELETE custom)</summary>
        public static async Task<int> ExecuteAsync(this DatabaseContext db, string sql, object? param = null)
        {
            await db.EnsureOpenAsync();
            return await db.Connection.ExecuteAsync(sql, param, db.Transaction);
        }

        /// <summary>Scalar (COUNT, SUM...)</summary>
        public static async Task<T?> ScalarAsync<T>(this DatabaseContext db, string sql, object? param = null)
        {
            await db.EnsureOpenAsync();
            return await db.Connection.ExecuteScalarAsync<T>(sql, param, db.Transaction);
        }

        #endregion
    }
}
